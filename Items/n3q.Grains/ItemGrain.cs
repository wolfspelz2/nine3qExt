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

        #region Interface

        public Task<Guid> GetStreamId() { return Task.FromResult(_streamId); }
        public Task<string> GetStreamNamespace() { return Task.FromResult(_streamNamespace); }

        public Task<bool> GetBool(Pid pid)
        {
            return Task.FromResult((bool)Properties.Get(pid));
        }

        public Task AddToItemSet(Pid pid, string itemId)
        {
            var ids = (ItemIdSet)Properties.Get(pid);
            ids.Add(itemId);
            Properties.Set(pid, ids);
            return Task.CompletedTask;
        }

        public Task Set(Pid pid, string value)
        {
            Properties.Set(pid, value);
            return Task.CompletedTask;
        }

        //public async Task Transfer(string dest)
        //{
        //    await Aspect.Container(dest).AddItem(Id);
        //    //Properties.Set(Pid.Container, dest);
        //}

        #endregion
    }

    //public class Aspect
    //{
    //    public string Id { get; set; }

    //    internal static ContainerAspect Container(string id)
    //    {
    //        return new ContainerAspect(id);
    //    }
    //}

    //public class ContainerAspect : Aspect
    //{
    //    public ContainerAspect(string id)
    //    {
    //        Id = id;
    //    }

    //    public Task AddItem(string id)
    //    {
    //        Properties.Set(Pid.Container, dest);

    //        return Task.CompletedTask;
    //    }
    //}
}
