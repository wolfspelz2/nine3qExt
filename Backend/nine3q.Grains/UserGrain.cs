using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Orleans;
using nine3q.GrainInterfaces;
using nine3q.Items;

namespace nine3q.Grains
{
    public class UserGrain : Grain, IUser
    {
        private readonly Dictionary<string, string> item1 = new Dictionary<string, string> {
            ["name"] = "General Sherman",
            ["avatarUrl"] = "https://weblin-avatar.dev.sui.li/items/baum/avatar.xml",
            ["rezzing"] = "false",
            ["rezzed"] = "false",
            ["room"] = "",
        };

        private string Id => this.GetPrimaryKeyString();
        private readonly Guid _streamId = Guid.NewGuid();

        private IInventory Inventory => GrainFactory.GetGrain<IInventory>(Id);
        private IInventory RemoteInventory(string key) => GrainFactory.GetGrain<IInventory>(key);
        private IUser User(string userId) => GrainFactory.GetGrain<IUser>(userId);
        private IRoom Room(string roomId) => GrainFactory.GetGrain<IRoom>(roomId);

        #region Interface

        public Task<Guid> GetStreamId() { return Task.FromResult(_streamId); }

        public async Task DropItem(long itemId, string roomId, long posX, string destinationUrl)
        {
            var props = await Inventory.GetItemProperties(itemId, new PidList { Pid.RezableAspect });

            if (!props.GetBool(Pid.RezableAspect)) { throw new SurfaceException(Id, itemId, SurfaceNotification.Fact.NotRezzed, SurfaceNotification.Reason.ItemNotRezable); }

            var setProps = new PropertySet { [Pid.Owner] = Id };
            var delProps = new PidList();
            var transferredId = await TransferItem(itemId, Id, roomId, ItemId.NoItem, 0, setProps, delProps);

            await Room(roomId).RezItem(transferredId, posX, destinationUrl);
        }

        public async Task PickupItem(long itemId, string roomId)
        {
            var props = await RemoteInventory(roomId).GetItemProperties(itemId, new PidList { Pid.RezableAspect });

            if (!props.GetBool(Pid.RezableAspect)) { throw new SurfaceException(roomId, itemId, SurfaceNotification.Fact.NotDerezzed, SurfaceNotification.Reason.ItemNotRezable); }

            await Room(roomId).DerezItem(itemId);

            // Transfer back before confirmation from xmpp rooom
            var setProps = new PropertySet { };
            var delProps = new PidList { Pid.Owner, Pid.RezzedX, Pid.IsRezzing, Pid.IsRezzed, Pid.IsDerezzing, };
            var transferredId = await TransferItem(itemId, roomId, Id, ItemId.NoItem, 0, setProps, delProps);
        }

        #endregion

        #region Internal

        public async Task<long> TransferItem(long id, string sourceInventory, string destInventory, long containerId, long slot, PropertySet setProperties, PidList removeProperties)
        {
            var source = GrainFactory.GetGrain<IInventory>(sourceInventory);
            var dest = GrainFactory.GetGrain<IInventory>(destInventory);
            var sourceId = id;
            var destId = ItemId.NoItem;

            try {
                var transfer = await source.BeginItemTransfer(sourceId);
                if (transfer.Count == 0) {
                    throw new Exception("BeginItemTransfer: no data");
                }

                var map = await dest.ReceiveItemTransfer(id, containerId, slot, transfer, setProperties, removeProperties);
                destId = map[sourceId];

                await dest.EndItemTransfer(destId);

                await source.EndItemTransfer(sourceId);
            } catch (Exception ex) {
                if (destId != ItemId.NoItem) {
                    await dest.CancelItemTransfer(destId);
                }

                await source.CancelItemTransfer(sourceId);
                throw ex;
            }

            return destId;
        }

        #endregion

        #region Lifecycle

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();

            //await ActivateInventorySubscription();
            //await InitializeCustomized();
        }

        #endregion

    }
}