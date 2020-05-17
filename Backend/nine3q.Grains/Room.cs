using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Orleans;
using nine3q.GrainInterfaces;

namespace nine3q.Grains
{
    public class Room : Grain, IRoom
    {
        private Dictionary<string, string> item2;

        private readonly Guid _streamId = Guid.NewGuid();
        public Task<Guid> GetStreamId() { return Task.FromResult(_streamId); }

        public async Task<long> ReceiveItem(long long, Dictionary<string, string> props)
        {
            await Task.CompletedTask;
            item2 = props;
            return 2;
        }

        public async Task RezItem(long long)
        {
            var streamProvider = GetStreamProvider(RoomStream.Provider);
            var stream = streamProvider.GetStream<RoomEvent>(_streamId, RoomStream.NamespaceEvents);
            await stream.OnNextAsync(new RoomEvent(RoomEvent.Type.Rez, this.GetPrimaryKeyString(), long));
        }

        public Task<string> GetItemProperty(long long, string key)
        {
            return Task.FromResult(item2[key]);
        }
    }
}