using System;
using System.Threading.Tasks;
using Orleans;
using GrainInterfaces;
using System.Collections.Generic;

namespace Grains
{
    public class Room : Grain, IRoom
    {
        private Dictionary<string, string> item2;

        private readonly Guid _streamId = Guid.NewGuid();
        public Task<Guid> GetStreamId() { return Task.FromResult(_streamId); }

        public async Task<long> ReceiveItem(long itemId, Dictionary<string, string> props)
        {
            await Task.CompletedTask;
            item2 = props;
            return 2;
        }

        public async Task RezItem(long itemId)
        {
            var streamProvider = GetStreamProvider(RoomStream.Provider);
            var stream = streamProvider.GetStream<RoomEvent>(_streamId, RoomStream.NamespaceEvents);
            await stream.OnNextAsync(new RoomEvent(RoomEvent.Type.Rez, this.GetPrimaryKeyString(), itemId));
        }

        public Task<string> GetItemProperty(long itemId, string key)
        {
            return Task.FromResult(item2[key]);
        }
    }
}