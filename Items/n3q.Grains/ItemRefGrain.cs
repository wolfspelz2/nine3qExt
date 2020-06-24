using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using n3q.GrainInterfaces;
using n3q.StorageProviders;
using n3q.Tools;

namespace n3q.Grains
{
    [Serializable]
    public class ItemRefState
    {
        public string ItemId;
    }

    public class ItemRefGrain : Grain, IItemRef
    {
        readonly IPersistentState<ItemRefState> _state;

        public DateTime Time { get; set; } = DateTime.MinValue;
        public CachedStringOptions.Persistence Persistence { get; set; } = CachedStringOptions.Persistence.Transient;

        public ItemRefGrain(
            [PersistentState("ItemRef", AzureReflectingTableStorage.StorageProviderName)] IPersistentState<ItemRefState> state
            )
        {
            _state = state;
        }

        #region Interface

        public async Task SetItem(string itemId)
        {
            _state.State.ItemId = itemId;
            await _state.WriteStateAsync();
        }

        public Task<string> GetItem()
        {
            return Task.FromResult(_state.State.ItemId);
        }

        public async Task Delete()
        {
            await DeletePersistentStorage();
            await Deactivate();
        }

        #endregion

        #region For tests

        public async Task DeletePersistentStorage()
        {
            await _state.ClearStateAsync();
        }

        public async Task ReloadPersistentStorage()
        {
            await _state.ReadStateAsync();
        }

        public Task Deactivate()
        {
            base.DeactivateOnIdle();
            return Task.CompletedTask;
        }

        #endregion
    }
}