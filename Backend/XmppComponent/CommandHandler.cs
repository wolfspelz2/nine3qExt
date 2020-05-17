using System;
using Orleans;
using nine3q.GrainInterfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using Orleans.Streams;

namespace XmppComponent
{
    internal class CommandHandler: IAsyncObserver<RoomEvent>
    {
        public Connection Connection { get; internal set; }

        private readonly string _host;
        private readonly IClusterClient _client;

        Dictionary<string, StreamSubscriptionHandle<RoomEvent>> roomEventsSubscriptionHandles = new Dictionary<string, StreamSubscriptionHandle<RoomEvent>>();

        public CommandHandler(string host, IClusterClient client)
        {
            _host = host;
            _client = client;
        }

        public async Task HandleCommand(Command cmd)
        {
            try {
                var method = cmd.ContainsKey("method") ? cmd["method"] : "";
                switch (method) {
                    case "rez": await HandleRez(cmd); break;
                }
            } catch (Exception ex) {
                Log.Error(ex);
            }
        }

        private async Task HandleRez(Command cmd)
        {
            var userId = cmd.ContainsKey("user") ? cmd["user"] : "";
            var haslong = long.TryParse(cmd.ContainsKey("item") ? cmd["item"] : "", out long long);
            var roomId = cmd.ContainsKey("room") ? cmd["room"] : "";
            var hasX = int.TryParse(cmd.ContainsKey("x") ? cmd["x"] : "", out int x);

            if (!string.IsNullOrEmpty(userId) && haslong && !string.IsNullOrEmpty(roomId) && hasX) {
                var room = _client.GetGrain<IRoom>(roomId);
                var streamProvider = _client.GetStreamProvider(RoomStream.Provider);
                var stream = streamProvider.GetStream<RoomEvent>(await room.GetStreamId(), RoomStream.NamespaceEvents);
                if (!roomEventsSubscriptionHandles.ContainsKey(roomId)) {
                    roomEventsSubscriptionHandles[roomId] = await stream.SubscribeAsync(this);
                }

                var user = _client.GetGrain<IUser>(userId);
                await user.DropItem(long, roomId, x);
            }
        }

        //private async Task HandleRoomEvent(IRoom room, RoomEvent roomEvent)
        //{
        //    try {
        //        if (roomEvent.type == RoomEvent.Type.Rez) {
        //            var roomId = room.GetPrimaryKeyString();
        //            var nick = await room.GetItemProperty(roomEvent.long, "name");
        //            var avatarUrl = await room.GetItemProperty(roomEvent.long, "avatarUrl");

        //            Connection?.Send(@$"<presence to='{roomId}/{nick}' from='{roomEvent.long}@{_host}/backend'>
        //               <x xmlns='http://jabber.org/protocol/muc'>
        //                 <history seconds='0' maxchars='0' maxstanzas='0'/>
        //               </x>
        //               <x xmlns='firebat:user:identity' jid='{roomEvent.long}@{_host}' src='https://avatar.weblin.sui.li/identity/?avatarUrl={avatarUrl}&nickname={nick}' digest='1' />
        //            </presence>");
        //        }
        //    } catch (Exception ex) {
        //        throw;
        //    }
        //}

        public async Task OnNextAsync(RoomEvent roomEvent, StreamSequenceToken token = null)
        {
            try {
                if (roomEvent.type == RoomEvent.Type.Rez) {
                    var roomId = roomEvent.roomId;
                    var room = _client.GetGrain<IRoom>(roomId);
                    var nick = await room.GetItemProperty(roomEvent.long, "name");
                    var avatarUrl = await room.GetItemProperty(roomEvent.long, "avatarUrl");

                    Connection?.Send(@$"<presence to='{roomId}/{nick}' from='{roomEvent.long}@{_host}/backend'>
                       <x xmlns='http://jabber.org/protocol/muc'>
                         <history seconds='0' maxchars='0' maxstanzas='0'/>
                       </x>
                       <x xmlns='firebat:user:identity' jid='{roomEvent.long}@{_host}' src='https://avatar.weblin.sui.li/identity/?avatarUrl={avatarUrl}&amp;nickname={nick}' digest='1' />
                    </presence>");
                }
            } catch (Exception ex) {
                Log.Error(ex);
            }
        }

        public Task OnCompletedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            return Task.CompletedTask;
        }
    }
}