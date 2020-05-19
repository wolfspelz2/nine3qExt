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
using System.Collections.Concurrent;
using nine3q.Tools;

namespace XmppComponent
{
    internal partial class Controller : IAsyncObserver<RoomEvent>
    {
        readonly string _componentHost;
        readonly string _componentDomain;
        readonly int _componentPort;
        readonly string _componentSecret;

        readonly IClusterClient _clusterClient;

        readonly object _mutex = new object();
        readonly Dictionary<string, StreamSubscriptionHandle<RoomEvent>> _roomEventsSubscriptionHandles = new Dictionary<string, StreamSubscriptionHandle<RoomEvent>>();
        readonly Dictionary<string, ManagedRoom> _rooms = new Dictionary<string, ManagedRoom>();

        Connection _conn;

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

        void StartConnectionNewThread()
        {
            Task.Run(async () => {
                _conn = new Connection(
                    _componentHost,
                    _componentDomain,
                    _componentPort,
                    _componentSecret,
                    async cmd => { await Connection_OnMessage(cmd); },
                    async cmd => { await Connection_OnPresence(cmd); },
                    conn => { Connection_OnClosed(conn); }
                    );

                await _conn.Run();
            });
        }

        #region Shortcuts

        IUser User(string key)
        {
            Contract.Requires(_clusterClient != null);
            return _clusterClient.GetGrain<IUser>(key);
        }

        IRoom Room(string key)
        {
            Contract.Requires(_clusterClient != null);
            return _clusterClient.GetGrain<IRoom>(key);
        }

        IInventory Inventory(string key)
        {
            Contract.Requires(_clusterClient != null);
            return _clusterClient.GetGrain<IInventory>(key);
        }

        private async Task<IAsyncStream<T>> Stream<T>(string roomId, string streamProvider, string streamNamespace)
        {
            var provider = _clusterClient.GetStreamProvider(streamProvider);
            var streamId = await Room(roomId).GetStreamId();
            var stream = provider.GetStream<T>(streamId, streamNamespace);
            return stream;
        }

        #endregion

        #region IAsyncObserver<RoomEvent>

        public async Task OnNextAsync(RoomEvent roomEvent, StreamSequenceToken token = null)
        {
            try {
                switch (roomEvent.type) {
                    case RoomEvent.Type.RezItem: await RoomEventStream_OnRez(roomEvent); break;
                    case RoomEvent.Type.DerezItem: await RoomEventStream_OnDerez(roomEvent); break;
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

        public async Task RoomEventStream_OnRez(RoomEvent roomEvent)
        {
            var roomId = roomEvent.roomId;
            var itemId = roomEvent.itemId;

            var props = await Inventory(roomId).GetItemProperties(itemId, new PidList { Pid.Name, Pid.AnimationsUrl, Pid.Image100Url });

            var name = props.GetString(Pid.Name);
            if (string.IsNullOrEmpty(name)) { name = "Item"; }

            var roomItemJid = new RoomItemJid(roomId, itemId, name);

            var animationsUrl = props.GetString(Pid.AnimationsUrl);
            var imageUrl = string.IsNullOrEmpty(animationsUrl) ? props.GetString(Pid.Image100Url) : "";

            var to = roomItemJid.Full;
            var from = $"{itemId}@{_componentDomain}/backend";
            var identityJid = $"{itemId}@{_componentDomain}";
            var identityDigest = Math.Abs(string.GetHashCode(name + animationsUrl, StringComparison.InvariantCulture)).ToString(CultureInfo.InvariantCulture);

            var name_UrlEncoded = WebUtility.UrlEncode(name);
            var animationsUrl_UrlEncoded = string.IsNullOrEmpty(animationsUrl) ? "" : WebUtility.UrlEncode(animationsUrl);
            var digest_UrlEncoded = WebUtility.UrlEncode(identityDigest);
            var identitySrc = $"https://avatar.weblin.sui.li/identity/?avatarUrl={animationsUrl_UrlEncoded}&nickname={name_UrlEncoded}&digest={digest_UrlEncoded}";

            var to_XmlEncoded = WebUtility.HtmlEncode(to);
            var from_XmlEncoded = WebUtility.HtmlEncode(from);
            var name_XmlEncoded = WebUtility.HtmlEncode(name_UrlEncoded);
            var animationsUrl_XmlEncoded = string.IsNullOrEmpty(animationsUrl) ? "" : WebUtility.HtmlEncode(animationsUrl);
            var imageUrl_XmlEncoded = string.IsNullOrEmpty(imageUrl) ? "" : WebUtility.HtmlEncode(imageUrl);
            var identitySrc_XmlEncoded = WebUtility.HtmlEncode(identitySrc);
            var identityDigest_XmlEncoded = WebUtility.HtmlEncode(identityDigest);
            var identityJid_XmlEncoded = WebUtility.HtmlEncode(identityJid);

            var animationsUrl_Attribute = $"animationsUrl='{animationsUrl_XmlEncoded}'";
            var imageUrl_Attribute = $"imageUrl='{imageUrl_XmlEncoded}'";

            var roomItem = GetRoomItem(roomId, itemId);
            if (roomItem != null) {
                roomItem.Resource = roomItemJid.Resource;
                roomItem.State = RoomItem.RezState.Rezzing;
            }

            _conn?.Send(
@$"<presence to='{to_XmlEncoded}' from='{from_XmlEncoded}'>
    <x xmlns='vp.props' nickname='{name_XmlEncoded}' {(string.IsNullOrEmpty(animationsUrl) ? "" : animationsUrl_Attribute)} {(string.IsNullOrEmpty(imageUrl) ? "" : imageUrl_Attribute)} />
    <x xmlns='firebat:user:identity' jid='{identityJid_XmlEncoded}' src='{identitySrc_XmlEncoded}' digest='{identityDigest_XmlEncoded}' />
    <x xmlns='http://jabber.org/protocol/muc'><history seconds='0' maxchars='0' maxstanzas='0'/></x>
</presence>"
            );
        }

        public async Task RoomEventStream_OnDerez(RoomEvent roomEvent)
        {
            await Task.CompletedTask;

            var roomId = roomEvent.roomId;
            var itemId = roomEvent.itemId;

            var name = "";

            var to = new RoomItemJid(roomId, itemId, name).Full;
            var from = $"{itemId}@{_componentDomain}/backend";

            var to_XmlEncoded = WebUtility.HtmlEncode(to);
            var from_XmlEncoded = WebUtility.HtmlEncode(from);

            _conn?.Send($"<presence to='{to_XmlEncoded}' from='{from_XmlEncoded}' type='unavailable' />");

            await RemoveRoomItem(roomId, itemId);
        }

        #endregion

        #region Management

        private async Task<RoomItem> AddRoomItem(string roomId, long itemId)
        {
            var doSubscribe = false;
            var roomItem = (RoomItem)null;

            lock (_mutex) {
                var managedRoom = (ManagedRoom)null;
                if (!_rooms.ContainsKey(roomId)) {
                    _rooms[roomId] = new ManagedRoom(roomId);
                }
                managedRoom = _rooms[roomId];
                if (!managedRoom.Items.ContainsKey(itemId)) {
                    managedRoom.Items[itemId] = new RoomItem(itemId);
                }
                roomItem = managedRoom.Items[itemId];
            }

            if (doSubscribe) {
                var stream = await Stream<RoomEvent>(roomId, RoomStream.Provider, RoomStream.NamespaceEvents);
                var subscriptionHandle = await stream.SubscribeAsync(this);
                lock (_mutex) {
                    if (!_roomEventsSubscriptionHandles.ContainsKey(roomId)) {
                        _roomEventsSubscriptionHandles[roomId] = subscriptionHandle;
                    }
                }
            }

            return roomItem;
        }

        private RoomItem GetRoomItem(string roomId, long itemId)
        {
            var roomItem = (RoomItem)null;

            lock (_mutex) {
                if (_rooms.ContainsKey(roomId)) {
                    var managedRoom = _rooms[roomId];
                    if (managedRoom.Items.ContainsKey(itemId)) {
                        roomItem = managedRoom.Items[itemId];
                    }
                }
            }

            return roomItem;
        }

        private async Task RemoveRoomItem(string roomId, long itemId)
        {
            var doUnsubscribe = false;

            lock (_mutex) {
                if (_rooms.ContainsKey(roomId)) {
                    var managedRoom = _rooms[roomId];
                    if (managedRoom.Items.ContainsKey(itemId)) {
                        managedRoom.Items.Remove(itemId);
                        if (managedRoom.Items.Count == 0) {
                            doUnsubscribe = true;
                        }
                    }
                }
            }

            if (doUnsubscribe) {
                var subscriptionHandle = (StreamSubscriptionHandle<RoomEvent>)null;
                lock (_mutex) {
                    if (_roomEventsSubscriptionHandles.ContainsKey(roomId)) {
                        subscriptionHandle = _roomEventsSubscriptionHandles[roomId];
                        _roomEventsSubscriptionHandles.Remove(roomId);
                    }
                }
                await subscriptionHandle.UnsubscribeAsync();
            }
        }

        #endregion

        #region OnConnction

        async Task Connection_OnMessage(XmppMessage stanza)
        {
            try {
                switch (stanza.MessageType) {
                    case XmppMessageType.Normal: await Connection_OnNormalMessage(stanza); break;
                    case XmppMessageType.Groupchat: await Connection_OnGroupchatMessage(stanza); break;
                }
            } catch (Exception ex) {
                Log.Error(ex);
            }
        }

        async Task Connection_OnPresence(XmppPresence stanza)
        {
            try {
                switch (stanza.PresenceType) {
                    case XmppPresenceType.Available: await Connection_OnPresenceAvailable(stanza); break;
                    case XmppPresenceType.Unavailable: await Connection_OnPresenceUnavailable(stanza); break;
                }
            } catch (Exception ex) {
                Log.Error(ex);
            }
        }

        void Connection_OnClosed(Connection conn)
        {
            _conn = null;

            Thread.Sleep(3000);

            StartConnectionNewThread();
        }

        async Task Connection_OnNormalMessage(XmppMessage stanza)
        {
            try {
                var method = stanza.Cmd.ContainsKey("method") ? stanza.Cmd["method"] : "";
                switch (method) {
                    case "dropItem": await Connection_OnDropItem(stanza); break;
                    case "pickupItem": await Connection_OnPickupItem(stanza); break;
                    default: Log.Warning($"Unknown method={method}"); break;
                }
            } catch (Exception ex) {
                Log.Error(ex);
            }
        }

        async Task Connection_OnGroupchatMessage(XmppMessage stanza)
        {
            await Task.CompletedTask;
        }

        async Task Connection_OnPresenceAvailable(XmppPresence stanza)
        {
            var jid = new RoomItemJid(stanza.From);
            var roomId = jid.Room;
            var itemId = jid.Item;

            var roomItem = GetRoomItem(roomId, itemId);
            if (roomItem != null) {
                roomItem.State = RoomItem.RezState.Rezzed;
            }

            await Room(roomId).OnItemRezzed(itemId);
        }

        async Task Connection_OnPresenceUnavailable(XmppPresence stanza)
        {
            var jid = new RoomItemJid(stanza.From);
            var roomId = jid.Room;
            var itemId = jid.Item;

            var roomItem = GetRoomItem(roomId, itemId);
            if (roomItem != null) {
                if (roomItem.State == RoomItem.RezState.Derezzing) {
                    roomItem.State = RoomItem.RezState.Derezzed;

                    await Room(roomId).OnItemDerezzed(itemId);

                } else {
                    Log.Warning($"Unexpected presence-unavailable room={roomId} item={itemId}");
                }
            }
        }

        async Task Connection_OnDropItem(XmppMessage message)
        {
            var userId = message.Cmd.ContainsKey("user") ? message.Cmd["user"] : "";
            var itemId = ItemId.NoItem;
            _ = long.TryParse(message.Cmd.ContainsKey("item") ? message.Cmd["item"] : "0", out itemId);
            var roomId = message.Cmd.ContainsKey("room") ? message.Cmd["room"] : "";
            var hasX = long.TryParse(message.Cmd.ContainsKey("x") ? message.Cmd["x"] : "", out long posX);
            var destinationUrl = message.Cmd.ContainsKey("destination") ? message.Cmd["destination"] : "";

            if (!string.IsNullOrEmpty(userId) && itemId != ItemId.NoItem && !string.IsNullOrEmpty(roomId) && hasX) {

                itemId = await TestPrepareItemForDrop(userId, roomId);

                var roomItem = await AddRoomItem(roomId, itemId);
                if (roomItem != null) {
                    roomItem.State = RoomItem.RezState.Dropping;
                }

                await User(userId).DropItem(itemId, roomId, posX, destinationUrl);
            }
        }

        async Task Connection_OnPickupItem(XmppMessage message)
        {
            var userId = message.Cmd.ContainsKey("user") ? message.Cmd["user"] : "";
            var itemId = ItemId.NoItem;
            _ = long.TryParse(message.Cmd.ContainsKey("item") ? message.Cmd["item"] : "0", out itemId);
            var roomId = message.Cmd.ContainsKey("room") ? message.Cmd["room"] : "";

            if (!string.IsNullOrEmpty(userId) && itemId != ItemId.NoItem && !string.IsNullOrEmpty(roomId)) {
                await User(userId).PickupItem(itemId, roomId);
            }
        }

        #endregion

        async Task<long> TestPrepareItemForDrop(string userId, string roomId)
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