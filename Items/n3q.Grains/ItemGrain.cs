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

        readonly string _streamNamespace = ItemService.StreamNamespaceDefault;
        readonly Guid _streamId = ItemService.StreamGuidDefault;
        readonly IPersistentState<ItemState> _state;

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

            _state.State.Id = this.GetPrimaryKeyString();
            Properties = new PropertySet(_state.State.Properties);

            await _state.ReadStateAsync();
        }

        public override async Task OnDeactivateAsync()
        {
            await base.OnDeactivateAsync();
        }

        #endregion

        #region Changes

        //await _state.WriteStateAsync();

        #endregion

        #region Interface

        public Task Set(Pid pid, string value) { Properties.Set(pid, value); return Task.CompletedTask; }
        public Task Set(Pid pid, long value) { Properties.Set(pid, value); return Task.CompletedTask; }
        public Task Set(Pid pid, double value) { Properties.Set(pid, value); return Task.CompletedTask; }
        public Task Set(Pid pid, bool value) { Properties.Set(pid, value); return Task.CompletedTask; }
        public Task Set(Pid pid, ItemIdSet value) { Properties.Set(pid, value); return Task.CompletedTask; }

        public Task AddToItemSet(Pid pid, string itemId)
        {
            var ids = (ItemIdSet)Properties.Get(pid);
            ids.Add(itemId);
            Properties.Set(pid, ids);
            return Task.CompletedTask;
        }

        public Task DeleteFromItemSet(Pid pid, string itemId)
        {
            var ids = (ItemIdSet)Properties.Get(pid);
            ids.Remove(itemId);
            Properties.Set(pid, ids);
            return Task.CompletedTask;
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
            await _state.WriteStateAsync();
        }

        public async Task ReadPersistentStorage()
        {
            await _state.ReadStateAsync();
        }

        public async Task DeletePersistentStorage()
        {
            await _state.ClearStateAsync();
        }

        #endregion

    }
}
