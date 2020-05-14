using System;
using System.Threading.Tasks;
using Orleans;
using GrainInterfaces;
using System.Collections.Generic;

namespace Grains
{
    public class User : Grain, IUser
    {
        private Dictionary<string, string> item1 = new Dictionary<string, string> {
            ["name"] = "General Sherman",
            ["avatarUrl"] = "https://weblin-avatar.dev.sui.li/items/baum/avatar.xml",
            ["rezzing"] = "false",
            ["rezzed"] = "false",
            ["room"] = "",
        };

        private readonly Guid _streamId = Guid.NewGuid();
        public Task<Guid> GetStreamId() { return Task.FromResult(_streamId); }

        public async Task DropItem(long itemId, string roomId, int x)
        {
            var room = GrainFactory.GetGrain<IRoom>(roomId);
            var newItemId = await room.ReceiveItem(itemId, item1);
            //item1 = null;
            await room.RezItem(newItemId);
        }
    }
}