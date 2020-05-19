using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics.Contracts;
using Orleans;
using Orleans.Streams;
using nine3q.GrainInterfaces;
using nine3q.Items;
using System.Threading;

namespace XmppComponent
{
    internal class Controller : IAsyncObserver<RoomEvent>
    {
        private readonly string _componentHost;
        private readonly string _componentDomain;
        private readonly int _componentPort;
        private readonly string _componentSecret;

        private readonly IClusterClient _clusterClient;
        readonly Dictionary<string, StreamSubscriptionHandle<RoomEvent>> _roomEventsSubscriptionHandles = new Dictionary<string, StreamSubscriptionHandle<RoomEvent>>();

        private Connection _conn;

        public Controller(IClusterClient clusterClient, string componentHost, string componentDomain, int componentPort, string componentSecret)
        {
            _clusterClient = clusterClient;
            _componentHost = componentHost;
            _componentDomain = componentDomain;
            _componentPort = componentPort;
            _componentSecret = componentSecret;
        }

        public void Start()
        {
            StartConnectionNewThread();
        }

        public void StartConnectionNewThread()
        {
            Task.Run(async () => {
                _conn = new Connection(
                    _componentHost,
                    _componentDomain,
                    _componentPort,
                    _componentSecret,
                    async cmd => { await HandleConnectionCommand(cmd); },
                    conn => { HandleConnectionClosed(conn); }
                    );

                await _conn.Run();
            });
        }

        private IInventory Inventory(string key)
        {
            Contract.Requires(_clusterClient != null);
            return _clusterClient.GetGrain<IInventory>(key);
        }

        #region IAsyncObserver<RoomEvent>

        public async Task OnNextAsync(RoomEvent roomEvent, StreamSequenceToken token = null)
        {
            try {
                if (roomEvent.type == RoomEvent.Type.RezItem) {
                    var roomId = roomEvent.roomId;

                    var props = await Inventory(roomId).GetItemProperties(roomEvent.itemId, new PidList { Pid.Name, Pid.AnimationsUrl, Pid.Image100Url });

                    var nick = props.GetString(Pid.Name);
                    var animationsUrl = props.GetString(Pid.AnimationsUrl);
                    var imageUrl = string.IsNullOrEmpty(animationsUrl) ? props.GetString(Pid.Image100Url) : "";

                    var to = $"{roomId}/{nick}";
                    var from = $"{roomEvent.itemId}@{_componentDomain}/backend";
                    var itemJid = $"{roomEvent.itemId}@{_componentDomain}";
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

                    _conn?.Send(
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

        #endregion

        public async Task HandleConnectionCommand(Command cmd)
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

        internal void HandleConnectionClosed(Connection conn)
        {
            _conn = null;

            Thread.Sleep(3000);

            StartConnectionNewThread();
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

                var streamProvider = _clusterClient.GetStreamProvider(RoomStream.Provider);
                var stream = streamProvider.GetStream<RoomEvent>(await _clusterClient.GetGrain<IRoom>(roomId).GetStreamId(), RoomStream.NamespaceEvents);
                if (!_roomEventsSubscriptionHandles.ContainsKey(roomId)) {
                    _roomEventsSubscriptionHandles[roomId] = await stream.SubscribeAsync(this);
                }

                await _clusterClient.GetGrain<IUser>(userId).DropItem(itemId, roomId, posX, destinationUrl);
            }
        }

        private async Task<long> TestPrepareItemForDrop(string userId, string roomId)
        {
            // Cleanup
            await Inventory(userId).DeleteItem(await Inventory(userId).GetItemByName("General Sherman"));
            await Inventory(roomId).DeleteItem(await Inventory(roomId).GetItemByName("General Sherman"));

            return await Inventory(userId).CreateItem(new PropertySet {
                [Pid.Name] = "General Sherman",
                [Pid.AnimationsUrl] = "https://weblin-avatar.dev.sui.li/items/baum/avatar.xml",
                [Pid.RezableAspect] = true,
            });
        }

    }
}