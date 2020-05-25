using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Orleans;
using Orleans.Runtime;
using Orleans.Streams;
using nine3q.Tools;
using nine3q.Items;
using nine3q.GrainInterfaces;
using nine3q.StorageProviders;
using nine3q.Common;

namespace nine3q.Grains
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
        PropertySet Properties => new PropertySet(_state.State.Properties);

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

        #region Interface

        public Task<Guid> GetStreamId() { return Task.FromResult(_streamId); }
        public Task<string> GetStreamNamespace() { return Task.FromResult(_streamNamespace); }

        #endregion

        #region Lifecycle

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();

            _state.State.Id = this.GetPrimaryKeyString();

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

        #region Test/Maintanance

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
