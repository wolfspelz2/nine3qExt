using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Orleans;
using nine3q.GrainInterfaces;
using nine3q.Items;

namespace nine3q.Grains
{
    public class RoomGrain : Grain, IRoom
    {
        private string Id => this.GetPrimaryKeyString();
        private IInventory Inventory => GrainFactory.GetGrain<IInventory>(Id);

        private readonly Guid _streamId = Guid.NewGuid();
        public Task<Guid> GetStreamId() { return Task.FromResult(_streamId); }

        public async Task RezItem(long itemId, long posX, string destinationUrl)
        {
            await Inventory.SetItemProperties(itemId, new Items.PropertySet {
                [Pid.IsRezzing] = true,
                [Pid.RezzedX] = posX,
            });

            var streamProvider = GetStreamProvider(RoomStream.Provider);
            var stream = streamProvider.GetStream<RoomEvent>(_streamId, RoomStream.NamespaceEvents);
            await stream.OnNextAsync(new RoomEvent(RoomEvent.Type.RezItem, Id, itemId));
        }
    }
}