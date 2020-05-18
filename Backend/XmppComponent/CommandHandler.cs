using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using Orleans;
using Orleans.Streams;
using nine3q.Items;
using nine3q.GrainInterfaces;

namespace XmppComponent
{
    internal class CommandHandler : IAsyncObserver<RoomEvent>
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
                    case "dropItem": await HandleDropItem(cmd); break;
                }
            } catch (Exception ex) {
                Log.Error(ex);
            }
        }

        private async Task HandleDropItem(Command cmd)
        {
            var userId = cmd.ContainsKey("user") ? cmd["user"] : "";
            var hasItemId = long.TryParse(cmd.ContainsKey("item") ? cmd["item"] : "", out long itemId);
            var roomId = cmd.ContainsKey("room") ? cmd["room"] : "";
            var hasX = long.TryParse(cmd.ContainsKey("x") ? cmd["x"] : "", out long posX);
            var destinationUrl = cmd.ContainsKey("room") ? cmd["destination"] : "";

            if (!string.IsNullOrEmpty(userId) && hasItemId && !string.IsNullOrEmpty(roomId) && hasX) {
                var room = _client.GetGrain<IRoom>(roomId);
                var streamProvider = _client.GetStreamProvider(RoomStream.Provider);
                var stream = streamProvider.GetStream<RoomEvent>(await room.GetStreamId(), RoomStream.NamespaceEvents);
                if (!roomEventsSubscriptionHandles.ContainsKey(roomId)) {
                    roomEventsSubscriptionHandles[roomId] = await stream.SubscribeAsync(this);
                }

                var user = _client.GetGrain<IUser>(userId);
                await user.DropItem(itemId, roomId, posX, destinationUrl);
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
                if (roomEvent.type == RoomEvent.Type.RezItem) {
                    var roomId = roomEvent.roomId;
                    var inv = _client.GetGrain<IInventory>(roomId);

                    var props = await inv.GetItemProperties(roomEvent.itemId, new PidList { Pid.Name, Pid.AnimationsUrl, Pid.Image100Url });

                    var nick = props.GetString(Pid.Name);
                    var animationsUrl = props.GetString(Pid.AnimationsUrl);
                    var imageUrl = string.IsNullOrEmpty(animationsUrl) ? props.GetString(Pid.Image100Url) : "";

                    var to = $"{roomId}/{nick}";
                    var toXmlEncoded = WebUtility.HtmlEncode(to);

                    var from = $"{roomEvent.itemId}@{_host}/backend";
                    var fromXmlEncoded = WebUtility.HtmlEncode(from);

                    var itemJid = $"{roomEvent.itemId}@{_host}";
                    var itemJidXmlEncoded = WebUtility.HtmlEncode(itemJid);

                    var identityDigest = Math.Abs(string.GetHashCode(nick + animationsUrl, StringComparison.InvariantCulture)).ToString(CultureInfo.InvariantCulture);

                    var nickUrlEncoded = string.IsNullOrEmpty(nick) ? "xx" : WebUtility.UrlEncode(nick);
                    var animationsUrlUrlEncoded = string.IsNullOrEmpty(animationsUrl) ? "" : WebUtility.UrlEncode(animationsUrl);
                    var digestUrlEncoded = WebUtility.UrlEncode(identityDigest);
                    var identitySrc = $"https://avatar.weblin.sui.li/identity/?avatarUrl={animationsUrlUrlEncoded}&nickname={nickUrlEncoded}&digest={digestUrlEncoded}";

                    var animationsUrlXmlEncoded = string.IsNullOrEmpty(animationsUrl) ? "" : WebUtility.HtmlEncode(animationsUrl);
                    var imageUrlXmlEncoded = string.IsNullOrEmpty(imageUrl) ? "" : WebUtility.HtmlEncode(imageUrl);
                    var identitySrcXmlEncoded = WebUtility.HtmlEncode(identitySrc);
                    var identityDigestXmlEncoded = WebUtility.HtmlEncode(identityDigest);

                    var animationsUrlAttribute = $"animationsUrl='{animationsUrlXmlEncoded}";
                    var imageUrlAttribute = $"imageUrl='{imageUrlXmlEncoded}";

                    Connection?.Send(
@$"<presence to='{toXmlEncoded}' from='{fromXmlEncoded}'>
    <x xmlns='vp.props' nickname='{nickUrlEncoded}' {(string.IsNullOrEmpty(animationsUrl) ? "":animationsUrlAttribute)} {(string.IsNullOrEmpty(imageUrl) ? "" : imageUrlAttribute)} />
    <x xmlns='firebat:user:identity' jid='{itemJidXmlEncoded}' src='{identitySrcXmlEncoded}' digest='{identityDigestXmlEncoded}' />
    <x xmlns='http://jabber.org/protocol/muc'><history seconds='0' maxchars='0' maxstanzas='0'/></x>
</presence>"
                    );
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