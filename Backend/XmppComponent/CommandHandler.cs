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

                itemId = await TestPrepareItemForDrop(userId, roomId);

                var streamProvider = _client.GetStreamProvider(RoomStream.Provider);
                var stream = streamProvider.GetStream<RoomEvent>(await _client.GetGrain<IRoom>(roomId).GetStreamId(), RoomStream.NamespaceEvents);
                if (!roomEventsSubscriptionHandles.ContainsKey(roomId)) {
                    roomEventsSubscriptionHandles[roomId] = await stream.SubscribeAsync(this);
                }

                await _client.GetGrain<IUser>(userId).DropItem(itemId, roomId, posX, destinationUrl);
            }
        }

        private async Task<long> TestPrepareItemForDrop(string userId, string roomId)
        {
            var userItemId = await _client.GetGrain<IInventory>(userId).GetItemByName("General Sherman");
            var roomItemId = await _client.GetGrain<IInventory>(roomId).GetItemByName("General Sherman");
            await _client.GetGrain<IInventory>(userId).DeleteItem(userItemId);
            await _client.GetGrain<IInventory>(roomId).DeleteItem(roomItemId);

            return await _client.GetGrain<IInventory>(userId).CreateItem(new PropertySet {
                [Pid.Name] = "General Sherman",
                [Pid.AnimationsUrl] = "https://weblin-avatar.dev.sui.li/items/baum/avatar.xml",
                [Pid.RezableAspect] = true,
            });
        }

        public async Task OnNextAsync(RoomEvent roomEvent, StreamSequenceToken token = null)
        {
            try {
                if (roomEvent.type == RoomEvent.Type.RezItem) {
                    var roomId = roomEvent.roomId;

                    var props = await _client.GetGrain<IInventory>(roomId).GetItemProperties(roomEvent.itemId, new PidList { Pid.Name, Pid.AnimationsUrl, Pid.Image100Url });

                    var nick = props.GetString(Pid.Name);
                    var animationsUrl = props.GetString(Pid.AnimationsUrl);
                    var imageUrl = string.IsNullOrEmpty(animationsUrl) ? props.GetString(Pid.Image100Url) : "";

                    var to = $"{roomId}/{nick}";
                    var from = $"{roomEvent.itemId}@{_host}/backend";
                    var itemJid = $"{roomEvent.itemId}@{_host}";
                    var identityDigest = Math.Abs(string.GetHashCode(nick + animationsUrl, StringComparison.InvariantCulture)).ToString(CultureInfo.InvariantCulture);

                    var nick_UrlEncoded = string.IsNullOrEmpty(nick) ? "xx" : WebUtility.UrlEncode(nick);
                    var animationsUrl_UrlEncoded = string.IsNullOrEmpty(animationsUrl) ? "" : WebUtility.UrlEncode(animationsUrl);
                    var digest_UrlEncoded = WebUtility.UrlEncode(identityDigest);
                    var identitySrc = $"https://avatar.weblin.sui.li/identity/?avatarUrl={animationsUrl_UrlEncoded}&nickname={nick_UrlEncoded}&digest={digest_UrlEncoded}";

                    var to_XmlEncoded = WebUtility.HtmlEncode(to);
                    var from_XmlEncoded = WebUtility.HtmlEncode(from);
                    var itemJid_XmlEncoded = WebUtility.HtmlEncode(itemJid);
                    var nick_XmlEncoded = string.IsNullOrEmpty(nick) ? "" : WebUtility.HtmlEncode(nick);
                    var animationsUrl_XmlEncoded = string.IsNullOrEmpty(animationsUrl) ? "" : WebUtility.HtmlEncode(animationsUrl);
                    var imageUrl_XmlEncoded = string.IsNullOrEmpty(imageUrl) ? "" : WebUtility.HtmlEncode(imageUrl);
                    var identitySrc_XmlEncoded = WebUtility.HtmlEncode(identitySrc);
                    var identityDigest_XmlEncoded = WebUtility.HtmlEncode(identityDigest);

                    var animationsUrl_Attribute = $"animationsUrl='{animationsUrl_XmlEncoded}'";
                    var imageUrl_Attribute = $"imageUrl='{imageUrl_XmlEncoded}'";

                    Connection?.Send(
@$"<presence to='{to_XmlEncoded}' from='{from_XmlEncoded}'>
    <x xmlns='vp.props' nickname='{nick_XmlEncoded}' {(string.IsNullOrEmpty(animationsUrl) ? "" : animationsUrl_Attribute)} {(string.IsNullOrEmpty(imageUrl) ? "" : imageUrl_Attribute)} />
    <x xmlns='firebat:user:identity' jid='{itemJid_XmlEncoded}' src='{identitySrc_XmlEncoded}' digest='{identityDigest_XmlEncoded}' />
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