using System;
using System.Threading.Tasks;
using Orleans;
using nine3q.GrainInterfaces;
using nine3q.Items;

namespace nine3q.Grains
{
    public class RoomGrain : Grain, IRoom
    {
        private string Id => this.GetPrimaryKeyString();

        private readonly Guid _streamId = Guid.NewGuid();
        public Task<Guid> GetStreamId() { return Task.FromResult(_streamId); }

        private IInventory Inventory => GrainFactory.GetGrain<IInventory>(Id);

        private Orleans.Streams.IAsyncStream<T> Stream<T>(string streamNamespace)
        {
            var streamProvider = GetStreamProvider(RoomStream.Provider);
            var stream = streamProvider.GetStream<T>(_streamId, RoomStream.NamespaceEvents);
            return stream;
        }

        public async Task RezItem(long itemId, long posX, string destinationUrl)
        {
            var props = await Inventory.GetItemProperties(itemId, new PidList { Pid.RezableAspect, Pid.IsRezzed });

            if (!props.GetBool(Pid.RezableAspect)) { throw new SurfaceException(Id, itemId, SurfaceNotification.Fact.NotRezzed, SurfaceNotification.Reason.ItemNotRezable); }
            if (props.GetBool(Pid.IsRezzed)) { throw new SurfaceException(Id, itemId, SurfaceNotification.Fact.NotRezzed, SurfaceNotification.Reason.NotRezzed); }

            await Inventory.SetItemProperties(itemId, new PropertySet {
                [Pid.IsRezzing] = true,
                [Pid.RezzedX] = posX,
            });

            await Stream<RoomEvent>(RoomStream.NamespaceEvents).OnNextAsync(new RoomEvent(RoomEvent.Type.RezItem, Id, itemId));
        }

        public async Task OnItemRezzed(long itemId)
        {
            var props = await Inventory.GetItemProperties(itemId, new PidList { Pid.RezableAspect, Pid.IsRezzing });

            await Inventory.DeleteItemProperties(itemId, new PidList { Pid.IsRezzing });

            if (!props.GetBool(Pid.RezableAspect)) { throw new SurfaceException(Id, itemId, SurfaceNotification.Fact.NotRezzed, SurfaceNotification.Reason.ItemNotRezable); }
            if (!props.GetBool(Pid.IsRezzing)) { throw new SurfaceException(Id, itemId, SurfaceNotification.Fact.NotRezzed, SurfaceNotification.Reason.NotRezzed); }

            await Inventory.SetItemProperties(itemId, new PropertySet { [Pid.IsRezzed] = true, });
        }

        public async Task DerezItem(long itemId)
        {
            var props = await Inventory.GetItemProperties(itemId, new PidList { Pid.RezableAspect, Pid.IsRezzed });

            if (!props.GetBool(Pid.RezableAspect)) { throw new SurfaceException(Id, itemId, SurfaceNotification.Fact.NotDerezzed, SurfaceNotification.Reason.ItemNotRezable); }
            if (!props.GetBool(Pid.IsRezzed)) { throw new SurfaceException(Id, itemId, SurfaceNotification.Fact.NotDerezzed, SurfaceNotification.Reason.NotRezzed); }

            await Inventory.SetItemProperties(itemId, new PropertySet {
                [Pid.IsDerezzing] = true,
            });

            await Stream<RoomEvent>(RoomStream.NamespaceEvents).OnNextAsync(new RoomEvent(RoomEvent.Type.DerezItem, Id, itemId));
        }

        public async Task OnItemDerezzed(long itemId)
        {
            await Task.CompletedTask;

            // It's already gone, fetched by the user, before any confirmation from xmpp rooom

            //var props = await Inventory.GetItemProperties(itemId, new PidList { Pid.RezableAspect, Pid.IsRezzed });
            //if (!props.GetBool(Pid.RezableAspect)) { throw new SurfaceException(Id, itemId, SurfaceNotification.Fact.NotDerezzed, SurfaceNotification.Reason.ItemNotRezable); }
            //if (!props.GetBool(Pid.IsRezzed)) { throw new SurfaceException(Id, itemId, SurfaceNotification.Fact.NotDerezzed, SurfaceNotification.Reason.NotRezzed); }
            //await Inventory.DeleteItemProperties(itemId, new PidList { Pid.IsRezzed });
        }
    }
}