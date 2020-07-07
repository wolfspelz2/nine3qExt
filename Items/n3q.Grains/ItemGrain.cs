using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
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
    class ItemGrain : Grain, IItem
    //, IAsyncObserver<ItemUpdate>
    {
        string Id => this.GetPrimaryKeyString();
        public PropertySet Properties { get; set; }

        private readonly ILogger<ItemGrain> _logger;
        readonly IPersistentState<KeyValueStorageData> _state;

        readonly Guid _streamId = ItemService.StreamGuid;
        readonly string _streamNamespace = ItemService.StreamNamespace;
        IAsyncStream<ItemUpdate> _stream;
        IAsyncStream<ItemUpdate> ItemUpdateStream
        {
            get {
                if (_stream == null) {
                    _stream = GetStreamProvider(ItemService.StreamProvider).GetStream<ItemUpdate>(_streamId, _streamNamespace);
                }
                return _stream;
            }
        }

        Guid _transactionId = Guid.Empty;
        public PropertySet _savedProperties;
        List<ItemChange> _changes;

        public ItemGrain(
            ILogger<ItemGrain> logger,
            [PersistentState("Item", ItemAzureTableStorage.StorageProviderName)] IPersistentState<KeyValueStorageData> state
            )
        {
            _logger = logger;
            _state = state;
        }

        private IItem Item(string id) => GrainFactory.GetGrain<IItem>(id);

        #region Lifecycle

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
            ApplyState();
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
                var value = pair.Value;
                var length = Property.GetDefinition(pid).Type switch
                {
                    Property.Type.StringList => ((ValueList)value).Count,
                    Property.Type.StringStringMap => ((ValueMap)value).Count,
                    _ => 0L,
                };
                _changes.Add(new ItemChange(ItemChange.Mode.SetProperty, pid, value, length));
                Properties[pid] = value;
            }

            foreach (var pid in deleted) {
                if (Properties.ContainsKey(pid)) {
                    Properties.Delete(pid);
                }
                _changes.Add(new ItemChange(ItemChange.Mode.DeleteProperty, pid, null, 0L));
            }

            if (!InTransaction()) {
                await CommitChanges();
            }
        }

        public async Task AddToListProperty(Pid pid, PropertyValue value, Guid tid)
        {
            AssertCurrentTransaction(tid);

            if (!Properties.TryGetValue(pid, out var pv)) {
                pv = new PropertyValue();
                Properties[pid] = pv;
            }
            if (!pv.IsInList(value)) {
                pv.AddToList(value);
                _changes.Add(new ItemChange(ItemChange.Mode.AddToList, pid, value, ((ValueList)pv).Count));
            }

            if (!InTransaction()) {
                await CommitChanges();
            }
        }

        public async Task RemoveFromListProperty(Pid pid, PropertyValue value, Guid tid)
        {
            AssertCurrentTransaction(tid);

            if (Properties.TryGetValue(pid, out var pv)) {
                if (pv.IsInList(value)) {
                    pv.RemoveFromList(value);
                    _changes.Add(new ItemChange(ItemChange.Mode.RemoveFromList, pid, value, ((ValueList)pv).Count));
                }
            }

            if (!InTransaction()) {
                await CommitChanges();
            }
        }

        public async Task<PropertySet> GetProperties(PidSet pids, bool native = false)
        {
            var props = (PropertySet)null;

            if (pids == PidSet.All) {
                props = await GetPropertiesAll(native);
            } else if (pids.Count == 1 && (pids.Contains(Pid.MetaPublicAccess) || pids.Contains(Pid.MetaOwnerAccess))) {
                props = await GetPropertiesByAccess(pids.First(), native);
            } else if (pids.Count == 1 && (pids.Contains(Pid.MetaAspectGroup))) {
                props = await GetPropertiesByGroup(pids.First(), native);
            } else {
                props = await GetPropertiesByPid(pids, native);
            }

            var result = new PropertySet();
            if (props != null) {
                foreach (var pair in props) {
                    if (Property.IsEmpty(pair.Key, pair.Value)) {
                        result[pair.Key] = Property.DefaultValue(pair.Key);
                    } else {
                        result[pair.Key] = pair.Value;
                    }
                }
            }

            if (pids != null) {
                foreach (var pid in pids) {
                    if (!result.ContainsKey(pid) && Property.HasDefaultValue(pid)) {
                        result[pid] = Property.DefaultValue(pid);
                    }
                }
            }

            return result;
        }

        public async Task Delete(Guid tid)
        {
            if (InTransaction()) {
                if (!IsSameTransaction(tid)) {
                    throw new Exception($"Delete: #{Id} already in transaction={_transactionId} tid={tid}");
                } else {
                    // Begin same: ignore
                }
            }

            _changes.Add(new ItemChange(ItemChange.Mode.DeleteItem, Pid.Unknown, PropertyValue.Empty, 0L));

            if (!InTransaction()) {
                await DeletePersistentStorage();
                await Deactivate();
            }
        }

        public Task BeginTransaction(Guid tid)
        {
            if (InTransaction()) {
                if (!IsSameTransaction(tid)) {
                    throw new Exception($"BeginTransaction: #{Id} already in transaction={_transactionId} tid={tid}");
                } else {
                    // Begin same: ignore
                }
            }

            _logger.LogInformation($"{nameof(BeginTransaction)} #{Id} {tid} ");

            _transactionId = tid;
            _changes = new List<ItemChange>();
            _savedProperties = Properties.Clone();

            return Task.CompletedTask;
        }

        public async Task EndTransaction(Guid tid, bool success)
        {
            if (InTransaction()) {
                if (!IsSameTransaction(tid)) {
                    throw new Exception($"EndTransaction: #{Id} in different transaction={_transactionId} tid={tid}");
                } else {
                    _logger.LogInformation($"{nameof(EndTransaction)} #{Id} {tid} ");

                    if (success) {
                        await CommitChanges();
                    } else {
                        CancelChanges();
                    }

                    _transactionId = Guid.Empty;
                    _changes = null;
                }
            }
        }

        #endregion

        #region Internal

        private void AssertCurrentTransaction(Guid tid)
        {
            if (InTransaction()) {
                if (!IsSameTransaction(tid)) {
                    throw new Exception($"AssertCurrentTransaction: #{Id} in different transaction={_transactionId} tid={tid}");
                }
            } else {
                _changes = new List<ItemChange>();
                _logger.LogInformation($"Transaction: #{Id} {tid} ");
            }
        }

        private bool InTransaction() => _transactionId != Guid.Empty;
        private bool IsSameTransaction(Guid tid) => tid == _transactionId;

        private async Task<PropertySet> GetPropertiesAll(bool native = false)
        {
            var result = (PropertySet)null;

            if (native) {
                result = Properties;
            } else {
                var templateId = (string)Properties.Get(Pid.Template);
                if (Has.Value(templateId)) {
                    result = await Item(templateId).GetProperties(PidSet.All);
                    result = FilterTemplateProperties(result);
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
                var templateId = (string)Properties.Get(Pid.Template);
                if (Has.Value(templateId)) {
                    result = await Item(templateId).GetProperties(pids);
                    result = FilterTemplateProperties(result);
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

        private async Task<PropertySet> GetPropertiesByGroup(Pid groupPid, bool native = false)
        {
            var result = new PropertySet();

            var group = Property.Group.Unknown;
            switch (groupPid) {
                case Pid.MetaAspectGroup: group = Property.Group.Aspect; break;
            }

            if (native) {
                CopyPropertiesByGroup(result, group);
            } else {
                var templateId = (string)Properties.Get(Pid.Template);
                if (Has.Value(templateId)) {
                    result = await Item(templateId).GetProperties(new PidSet { groupPid });
                    result = FilterTemplateProperties(result);
                }

                result ??= new PropertySet();
                CopyPropertiesByGroup(result, group);
            }

            return result;
        }

        private void CopyPropertiesByGroup(PropertySet result, Property.Group group)
        {
            foreach (Pid pid in Enum.GetValues(typeof(Pid))) {
                if (Property.Definitions[pid].Group == group) {
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
                case Pid.MetaOwnerAccess: access = Property.Access.Owner; break;
                case Pid.MetaPublicAccess: access = Property.Access.Public; break;
            }

            if (native) {
                CopyPropertiesByAccess(result, access);
            } else {
                var templateId = (string)Properties.Get(Pid.Template);
                if (Has.Value(templateId)) {
                    result = await Item(templateId).GetProperties(new PidSet { accessPid });
                    result = FilterTemplateProperties(result);
                }

                result ??= new PropertySet();
                CopyPropertiesByAccess(result, access);
            }

            return result;
        }

        private void CopyPropertiesByAccess(PropertySet result, Property.Access access)
        {
            foreach (Pid pid in Enum.GetValues(typeof(Pid))) {
                if (Property.Definitions[pid].Access >= access) {
                    if (Properties.ContainsKey(pid)) {
                        result[pid] = Properties[pid];
                    }
                }
            }
        }

        private PropertySet FilterTemplateProperties(PropertySet props)
        {
            if (props.ContainsKey(Pid.Container)) {
                props.Remove(Pid.Container);
            }
            return props;
        }

        #endregion

        #region Changes

        private void CancelChanges()
        {
            Properties = _savedProperties;
        }

        private async Task CommitChanges()
        {
            foreach (var change in _changes) {
                var pid = change.Pid;
                var value = change.Value;

                switch (change.What) {
                    //case ItemChange.Mode.SetProperty: {
                    //    Properties[pid] = value;
                    //}
                    //break;

                    //case ItemChange.Mode.AddToList: {
                    //    if (Properties.TryGetValue(pid, out var pv)) {
                    //        pv.AddToList(value);
                    //    } else {
                    //        pv = new PropertyValue();
                    //        pv.AddToList(value);
                    //        Properties[pid] = pv;
                    //    }
                    //}
                    //break;

                    //case ItemChange.Mode.RemoveFromList: {
                    //    if (Properties.TryGetValue(pid, out var pv)) {
                    //        pv.RemoveFromList(value);
                    //    }
                    //}
                    //break;

                    //case ItemChange.Mode.DeleteProperty: {
                    //    if (Properties.ContainsKey(pid)) {
                    //        Properties.Delete(pid);
                    //    }
                    //}
                    //break;

                    case ItemChange.Mode.DeleteItem: {
                        await DeletePersistentStorage();
                        await Deactivate();
                    }
                    break;

                }
            }

            if (_changes.Count > 0) {
                // Notify subscribers
                _ = Properties.TryGetValue(Pid.Container, out var parentId);
                var update = new ItemUpdate(Id, parentId ?? PropertyValue.Empty, _changes);
                await ItemUpdateStream?.OnNextAsync(update);

                // Persist changes
                var persist = _changes.Aggregate(false, (current, change) => current |= PropertyMustBeSaved(change.Pid, change.Value));
                if (persist) {
                    await WritePersistentStorage();
                }
            }
        }

        public async Task WritePersistentStorage()
        {
            var propsToBeSaved = new KeyValueStorageData();
            foreach (var pair in Properties) {
                if (PropertyMustBeSaved(pair.Key, pair.Value)) {

                    object value = Property.GetDefinition(pair.Key).Storage switch
                    {
                        Property.Storage.Int => (long)pair.Value,
                        Property.Storage.Float => (double)pair.Value,
                        Property.Storage.Bool => (bool)pair.Value,
                        _ => (string)pair.Value,
                    };

                    propsToBeSaved.Add(pair.Key.ToString(), value);
                }
            }
            _state.State = propsToBeSaved;

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

        public void ApplyState()
        {
            Properties = new PropertySet();

            foreach (var pair in _state.State) {
                var pid = pair.Key.ToEnum(Pid.Unknown);
                if (pid != Pid.Unknown) {

                    PropertyValue pv;
                    if (false) {
                    } else if (pair.Value.GetType() == typeof(long)) {
                        pv = new PropertyValue((long)pair.Value);
                    } else if (pair.Value.GetType() == typeof(double)) {
                        pv = new PropertyValue((double)pair.Value);
                    } else if (pair.Value.GetType() == typeof(bool)) {
                        pv = new PropertyValue((bool)pair.Value);
                    } else {
                        pv = new PropertyValue(pair.Value.ToString());
                    }

                    Properties.Add(pid, pv);
                }
            }
        }

        #endregion

        #region Test / Maintanance / Operation

        public Task Deactivate()
        {
            base.DeactivateOnIdle();
            return Task.CompletedTask;
        }

        public async Task DeletePersistentStorage()
        {
            await _state.ClearStateAsync();
        }

        #endregion

    }
}
