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

    class ItemGrain : Grain
        , IItem
    //, IAsyncObserver<ItemUpdate>
    {
        string Id => _state.State.Id;
        public PropertySet Properties { get; set; }

        readonly string _streamNamespace = ItemService.StreamNamespace;
        readonly Guid _streamId = ItemService.StreamGuid;
        readonly IPersistentState<ItemState> _state;
        IAsyncStream<ItemUpdate> _stream;

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

        public async Task Set(Pid pid, string value)
        {
            Properties.Set(pid, value);
            await Update(ItemUpdate.Mode.PropertyChanged, pid, value);
        }

        public async Task Set(Pid pid, long value)
        {
            Properties.Set(pid, value);
            await Update(ItemUpdate.Mode.PropertyChanged, pid, value);
        }

        public async Task Set(Pid pid, double value)
        {
            Properties.Set(pid, value);
            await Update(ItemUpdate.Mode.PropertyChanged, pid, value);
        }

        public async Task Set(Pid pid, bool value)
        {
            Properties.Set(pid, value);
            await Update(ItemUpdate.Mode.PropertyChanged, pid, value);
        }

        public async Task Set(Pid pid, ItemIdSet value)
        {
            Properties.Set(pid, value);
            await Update(ItemUpdate.Mode.PropertyChanged, pid, value);
        }

        public async Task AddToItemSet(Pid pid, string itemId)
        {
            var ids = (ItemIdSet)Properties.Get(pid);
            ids.Add(itemId);
            Properties.Set(pid, ids);
            await Update(ItemUpdate.Mode.AddedToItemList, pid, itemId);
        }

        public async Task DeleteFromItemSet(Pid pid, string itemId)
        {
            var ids = (ItemIdSet)Properties.Get(pid);
            ids.Remove(itemId);
            Properties.Set(pid, ids);
            await Update(ItemUpdate.Mode.RemovedFromItemList, pid, itemId);
        }

        public Task<PropertyValue> Get(Pid pid)
        {
            return Task.FromResult(Properties.Get(pid));
        }

        public Task<string> GetString(Pid pid)
        {
            return Task.FromResult((string)Properties.Get(pid));
        }

        public Task<long> GetInt(Pid pid)
        {
            return Task.FromResult((long)Properties.Get(pid));
        }

        public Task<double> GetFloat(Pid pid)
        {
            return Task.FromResult((double)Properties.Get(pid));
        }

        public Task<bool> GetBool(Pid pid)
        {
            return Task.FromResult((bool)Properties.Get(pid));
        }

        public Task<string> GetItemId(Pid pid)
        {
            return Task.FromResult((string)Properties.Get(pid));
        }

        public Task<ItemIdSet> GetItemIdSet(Pid pid)
        {
            return Task.FromResult((ItemIdSet)Properties.Get(pid));
        }

        public async Task<PropertySet> GetProperties(PidSet pids, bool native = false)
        {
            if (pids == PidSet.All) {
                return await GetPropertiesAll(native);
            } else if (pids.Count == 1 && pids.Contains(Pid.PublicAccess)) {
                return GetPropertiesPublic(native);
            } else if (pids.Count == 1 && pids.Contains(Pid.OwnerAccess)) {
                return GetPropertiesOwner(native);
            }
            return GetPropertiesByPid(pids, native);
        }

        public Task Delete(Pid pid)
        {
            Properties.Delete(pid);
            return Task.CompletedTask;
        }

        #endregion

        #region Internal

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
            }

            result ??= new PropertySet();
            foreach (var pair in Properties) {
                result[pair.Key] = pair.Value;
            }

            return result;
        }

        private PropertySet GetPropertiesByPid(PidSet pids, bool native = false)
        {
            return Properties;
        }

        public PropertySet GetPropertiesPublic(bool native = false)
        {
            return GetPropertiesByAccess(Property.Access.Public, native);
        }

        public PropertySet GetPropertiesOwner(bool native = false)
        {
            return GetPropertiesByAccess(Property.Access.Owner, native);
        }

        private PropertySet GetPropertiesByAccess(Property.Access access, bool native = false)
        {
            var result = new PropertySet();

            foreach (Pid pid in Enum.GetValues(typeof(Pid))) {
                if (Property.Definitions[pid].Access == Property.Access.Public) {
                    if (Properties.ContainsKey(pid)) {
                        result.Add(pid, Properties[pid]);
                    }
                }
            }

            return result;
        }

        #endregion

        #region Changes

        //WritePersistentStorage();

        async Task Update(ItemUpdate.Mode what, Pid pid, PropertyValue value)
        {
            // Persist changes
            if (PropertyMustBeSaved(pid, value)) {
                await WritePersistentStorage();
            }

            // Notify subscribers
            var update = new ItemUpdate(what, Id, pid, value);
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
