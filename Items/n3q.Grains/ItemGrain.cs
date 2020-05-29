using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Orleans;
using Orleans.Runtime;
using Orleans.Streams;
using n3q.Tools;
using n3q.Items;
using n3q.GrainInterfaces;
using n3q.StorageProviders;
using n3q.Common;

namespace n3q.Grains
{
    [Serializable]
    public class ItemState
    {
        public string Id;
        public Dictionary<Pid, string> Properties;
    }

    class ItemGrain : Grain, IItem
    //, IAsyncObserver<ItemUpdate>
    {
        string Id => this.GetPrimaryKeyString();
        public PropertySet Properties { get; set; }

        readonly string _streamNamespace = ItemService.StreamNamespace;
        readonly Guid _streamId = ItemService.StreamGuid;
        readonly IPersistentState<ItemState> _state;
        IAsyncStream<ItemUpdate> _stream;

        Guid _transactionId = Guid.Empty;
        List<PropertyChange> _changes;

        public ItemGrain(
            [PersistentState("Item", JsonFileStorage.StorageProviderName)] IPersistentState<ItemState> itemState
            )
        {
            _state = itemState;
        }

        private IItem Item(string id) => GrainFactory.GetGrain<IItem>(id);

        #region Lifecycle

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();

            await ReadPersistentStorage();

            var streamProvider = GetStreamProvider(ItemService.StreamProvider);
            _stream = streamProvider.GetStream<ItemUpdate>(_streamId, ItemService.StreamNamespace);
        }

        public override async Task OnDeactivateAsync()
        {
            await base.OnDeactivateAsync();
        }

        #endregion

        #region Interface

        public async Task ModifyProperties(PropertySet modified, PidSet deleted, Guid tid)
        {
            AssertCurrentTransaction(tid);

            foreach (var pair in modified) {
                var pid = pair.Key;
                _changes.Add(new PropertyChange(PropertyChange.Mode.SetProperty, pid, pair.Value));
            }

            foreach (var pid in deleted) {
                _changes.Add(new PropertyChange(PropertyChange.Mode.DeleteProperty, pid, null));
            }

            if (tid == Guid.Empty) {
                await ApplyChanges();
            }
        }

        public async Task AddToList(Pid pid, PropertyValue value, Guid tid)
        {
            AssertCurrentTransaction(tid);

            if (Properties.TryGetValue(pid, out var pv)) {
                if (!pv.IsInList(value)) {
                    _changes.Add(new PropertyChange(PropertyChange.Mode.AddToList, pid, value));
                }
            } else {
                _changes.Add(new PropertyChange(PropertyChange.Mode.AddToList, pid, value));
            }

            if (tid == Guid.Empty) {
                await ApplyChanges();
            }
        }

        public async Task DeleteFromList(Pid pid, PropertyValue value, Guid tid)
        {
            AssertCurrentTransaction(tid);

            if (Properties.TryGetValue(pid, out var pv)) {
                if (pv.IsInList(value)) {
                    _changes.Add(new PropertyChange(PropertyChange.Mode.RemoveFromList, pid, value));
                }
            }

            if (tid == Guid.Empty) {
                await ApplyChanges();
            }
        }

        public async Task<PropertySet> GetProperties(PidSet pids, bool native = false)
        {
            //return await Impl.GetProperties(pids, native);
            if (pids == PidSet.All) {
                return await GetPropertiesAll(native);
            } else if (pids.Count == 1 && (pids.Contains(Pid.PublicAccess) || pids.Contains(Pid.OwnerAccess))) {
                return await GetPropertiesByAccess(pids.First(), native);
            }
            return await GetPropertiesByPid(pids, native);
        }

        public Task BeginTransaction(Guid tid)
        {
            if (HasCurrentTransaction()) {
                throw new Exception($"BeginTransaction: already in transaction current={_transactionId} tid={tid}");
            }

            _transactionId = tid;
            _changes = new List<PropertyChange>();

            return Task.CompletedTask;
        }

        public async Task EndTransaction(Guid tid, bool success)
        {
            if (HasCurrentTransaction()) {
                if (IsCurrentTransaction(tid)) {
                    if (success) {
                        await ApplyChanges();
                    }
                }
            }

            _transactionId = Guid.Empty;
            _changes = null;
        }

        #endregion

        #region Internal

        private void AssertCurrentTransaction(Guid tid)
        {
            if (HasCurrentTransaction()) {
                if (!IsCurrentTransaction(tid)) {
                    throw new Exception($"AssertCurrentTransaction: already in different transaction current={_transactionId} tid={tid}");
                }
            } else {
                _changes = new List<PropertyChange>();
            }
        }

        private bool HasCurrentTransaction() => _transactionId != Guid.Empty;
        private bool IsCurrentTransaction(Guid tid) => tid == _transactionId;

        private async Task<PropertySet> GetPropertiesAll(bool native = false)
        {
            var result = (PropertySet)null;

            if (native) {
                result = Properties;
            } else {
                var templateId = (string)Properties.Get(Pid.TemplateId);
                if (Has.Value(templateId)) {
                    result = await Item(templateId).GetProperties(PidSet.All);
                }

                result ??= new PropertySet();
                foreach (var pair in Properties) {
                    result[pair.Key] = pair.Value;
                }
            }

            return result;
        }

        private async Task<PropertySet> GetPropertiesByPid(PidSet pids, bool native = false)
        {
            var result = new PropertySet();

            if (native) {
                CopyPropertiesByPidSelection(result, pids);
            } else {
                var templateId = (string)Properties.Get(Pid.TemplateId);
                if (Has.Value(templateId)) {
                    result = await Item(templateId).GetProperties(pids);
                }

                result ??= new PropertySet();
                CopyPropertiesByPidSelection(result, pids);
            }

            return result;
        }

        private void CopyPropertiesByPidSelection(PropertySet result, PidSet pids)
        {
            foreach (Pid pid in pids) {
                if (pids.Contains(pid)) {
                    if (Properties.ContainsKey(pid)) {
                        result[pid] = Properties[pid];
                    }
                }
            }
        }

        private async Task<PropertySet> GetPropertiesByAccess(Pid accessPid, bool native = false)
        {
            var result = new PropertySet();

            var access = Property.Access.System;
            switch (accessPid) {
                case Pid.OwnerAccess: access = Property.Access.Owner; break;
                case Pid.PublicAccess: access = Property.Access.Public; break;
            }

            if (native) {
                CopyPropertiesByAccessLevel(result, access);
            } else {
                var templateId = (string)Properties.Get(Pid.TemplateId);
                if (Has.Value(templateId)) {
                    result = await Item(templateId).GetProperties(new PidSet { accessPid });
                }

                result ??= new PropertySet();
                CopyPropertiesByAccessLevel(result, access);
            }

            return result;
        }

        private void CopyPropertiesByAccessLevel(PropertySet result, Property.Access access)
        {
            foreach (Pid pid in Enum.GetValues(typeof(Pid))) {
                if (Property.Definitions[pid].Access >= access) {
                    if (Properties.ContainsKey(pid)) {
                        result[pid] = Properties[pid];
                    }
                }
            }
        }

        #endregion

        #region Changes

        //WritePersistentStorage();

        private async Task ApplyChanges()
        {
            foreach (var change in _changes) {
                var pid = change.Pid;
                var value = change.Value;

                switch (change.What) {
                    case PropertyChange.Mode.SetProperty: {
                        Properties[pid] = value;
                    }
                    break;

                    case PropertyChange.Mode.AddToList: {
                        if (Properties.TryGetValue(pid, out var pv)) {
                            pv.AddToList(value);
                        } else {
                            pv = new PropertyValue();
                            pv.AddToList(value);
                            Properties[pid] = pv;
                        }
                    }
                    break;

                    case PropertyChange.Mode.RemoveFromList: {
                        if (Properties.TryGetValue(pid, out var pv)) {
                            pv.RemoveFromList(value);
                        }
                    }
                    break;

                    case PropertyChange.Mode.DeleteProperty: {
                        if (Properties.ContainsKey(pid)) {
                            Properties.Delete(pid);
                        }
                    }
                    break;
                }
            }

            // Persist changes
            var persist = _changes.Aggregate(false, (current, change) => current |= PropertyMustBeSaved(change.Pid, change.Value));
            if (persist) {
                await WritePersistentStorage();
            }

            // Notify subscribers
            var update = new ItemUpdate(Id, _changes);
            await _stream?.OnNextAsync(update);
        }

        #endregion

        #region Test / Maintanance / Operation

        public Task<Guid> GetStreamId() { return Task.FromResult(_streamId); }
        public Task<string> GetStreamNamespace() { return Task.FromResult(_streamNamespace); }

        public Task Deactivate()
        {
            base.DeactivateOnIdle();
            return Task.CompletedTask;
        }

        public async Task WritePersistentStorage()
        {
            _state.State.Id = this.GetPrimaryKeyString();

            var propsToBeSaved = new Dictionary<Pid, string>();
            foreach (var pair in Properties) {
                if (PropertyMustBeSaved(pair.Key, pair.Value)) {
                    propsToBeSaved.Add(pair.Key, pair.Value);
                }
            }
            _state.State.Properties = propsToBeSaved;

            await _state.WriteStateAsync();
        }

        private bool PropertyMustBeSaved(Pid pid, PropertyValue value)
        {
            return Property.GetDefinition(pid).Persistence switch
            {
                Property.Persistence.Unknown => false,
                Property.Persistence.Fixed => false,
                Property.Persistence.Transient => false,
                Property.Persistence.Persistent => true,
                Property.Persistence.Slow => true,
                Property.Persistence.Unload => true,
                _ => true,
            };
        }

        public async Task ReadPersistentStorage()
        {
            await _state.ReadStateAsync();

            Properties = new PropertySet(_state.State.Properties);
        }

        public async Task DeletePersistentStorage()
        {
            await _state.ClearStateAsync();
        }

        #endregion

    }
}
