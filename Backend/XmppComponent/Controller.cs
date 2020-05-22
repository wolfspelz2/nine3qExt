using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Linq;
using Orleans;
using nine3q.Items;
using nine3q.GrainInterfaces;
using nine3q.Frontend;

namespace XmppComponent
{
    internal partial class Controller
    {
        readonly string _componentHost;
        readonly string _componentDomain;
        readonly int _componentPort;
        readonly string _componentSecret;

        readonly IClusterClient _clusterClient;

        readonly object _mutex = new object();
        //readonly Dictionary<string, StreamSubscriptionHandle<RoomEvent>> _roomEventsSubscriptionHandles = new Dictionary<string, StreamSubscriptionHandle<RoomEvent>>();
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

        internal void Send(string line)
        {
            if (_conn != null) {
                _conn.Send(line);
            }
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

        IInventory Inventory(string key)
        {
            Contract.Requires(_clusterClient != null);
            return _clusterClient.GetGrain<IInventory>(key);
        }

        //private async Task<IAsyncStream<T>> Stream<T>(string roomId, string streamProvider, string streamNamespace)
        //{
        //    var provider = _clusterClient.GetStreamProvider(streamProvider);
        //    var streamId = await Room(roomId).GetStreamId();
        //    var stream = provider.GetStream<T>(streamId, streamNamespace);
        //    return stream;
        //}

        #endregion

        #region Management

        RoomItem AddRoomItem(string roomId, long itemId)
        {
            var roomItem = (RoomItem)null;

            lock (_mutex) {
                var managedRoom = (ManagedRoom)null;
                if (!_rooms.ContainsKey(roomId)) {
                    _rooms[roomId] = new ManagedRoom(roomId);
                }
                managedRoom = _rooms[roomId];

                roomItem = managedRoom.Items.Where(ri => ri.ItemId == itemId).FirstOrDefault();
                if (roomItem == null) {
                    roomItem = new RoomItem(roomId, itemId);
                    managedRoom.Items.Add(roomItem);
                }
            }

            return roomItem;
        }

        RoomItem GetRoomItem(string roomId, long itemId)
        {
            var roomItem = (RoomItem)null;

            lock (_mutex) {
                if (_rooms.ContainsKey(roomId)) {
                    var managedRoom = _rooms[roomId];
                    roomItem = managedRoom.Items.Where(ri => ri.ItemId == itemId).FirstOrDefault();
                }
            }

            return roomItem;
        }

        void RemoveRoomItem(string roomId, long itemId)
        {
            lock (_mutex) {
                if (_rooms.ContainsKey(roomId)) {
                    var managedRoom = _rooms[roomId];
                    var roomItem = managedRoom.Items.Where(ri => ri.ItemId == itemId).FirstOrDefault();
                    if (roomItem != null) {
                        managedRoom.Items.Remove(roomItem);
                        if (managedRoom.Items.Count == 0) {
                            _rooms.Remove(roomId);
                        }
                    }
                }
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
            if (roomItem == null) {
                // Not my item
            } else {
                if (roomItem.State != RoomItem.RezState.Rezzing) {
                    Log.Warning($"Unexpected presence-available: room={roomId} item={itemId}", nameof(Connection_OnPresenceAvailable));
                } else {
                    Log.Info($"Joined {roomId} {itemId}", nameof(Connection_OnPresenceAvailable));
                    roomItem.State = RoomItem.RezState.Rezzed;
                    await Inventory(roomId).SetItemProperties(itemId, new PropertySet { [Pid.IsRezzed] = true });
                }
            }

            await Task.CompletedTask;
        }

        async Task Connection_OnPresenceUnavailable(XmppPresence stanza)
        {
            await Task.CompletedTask;
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
                //itemId = await TestPrepareItemForDrop(userId, roomId);

                var roomItem = AddRoomItem(roomId, itemId);
                if (roomItem != null) {
                    Log.Info($"Drop {roomId} {itemId}");

                    {
                        var props = await Inventory(userId).GetItemProperties(itemId, new PidList { Pid.RezableAspect });
                        if (!props.GetBool(Pid.RezableAspect)) { throw new SurfaceException(userId, itemId, SurfaceNotification.Fact.NotRezzed, SurfaceNotification.Reason.ItemNotRezable); }
                    }

                    var setProps = new PropertySet {
                        [Pid.Owner] = userId,
                        [Pid.IsRezzing] = true,
                        [Pid.RezzedX] = posX,
                    };
                    var transferredItemId = await TransferItem(itemId, userId, roomId, ItemId.NoItem, 0, setProps, new PidList());
                    roomItem.ItemId = transferredItemId;

                    await SendPresenceAvailable(roomItem, posX);

                    roomItem.State = RoomItem.RezState.Rezzing;
                }
            }
        }

        async Task Connection_OnPickupItem(XmppMessage message)
        {
            var userId = message.Cmd.ContainsKey("user") ? message.Cmd["user"] : "";
            var itemId = ItemId.NoItem;
            _ = long.TryParse(message.Cmd.ContainsKey("item") ? message.Cmd["item"] : "0", out itemId);
            var roomId = message.Cmd.ContainsKey("room") ? message.Cmd["room"] : "";

            if (!string.IsNullOrEmpty(userId) && itemId != ItemId.NoItem && !string.IsNullOrEmpty(roomId)) {

                var roomItem = GetRoomItem(roomId, itemId);
                if (roomItem != null) {
                    if (roomItem.State != RoomItem.RezState.Rezzed) {
                        Log.Warning($"Unexpected message-cmd-pickupItem: room={roomId} item={itemId}");
                        throw new SurfaceException(roomId, itemId, SurfaceNotification.Fact.NotDerezzed, SurfaceNotification.Reason.ItemNotRezzed);
                    } else {
                        Log.Info($"Pickup {roomId} {itemId}", nameof(Connection_OnPickupItem));

                        var props = await Inventory(roomId).GetItemProperties(itemId, new PidList { Pid.RezableAspect, Pid.IsRezzed });
                        if (!props.GetBool(Pid.RezableAspect)) { throw new SurfaceException(roomId, itemId, SurfaceNotification.Fact.NotDerezzed, SurfaceNotification.Reason.ItemNotRezable); }
                        if (!props.GetBool(Pid.IsRezzed)) { throw new SurfaceException(roomId, itemId, SurfaceNotification.Fact.NotDerezzed, SurfaceNotification.Reason.ItemNotRezzed); }

                        await SendPresenceUnvailable(roomItem);

                        // Transfer back before confirmation from xmpp room
                        var delProps = new PidList { Pid.Owner, Pid.RezzedX, Pid.IsRezzing, Pid.IsRezzed };
                        var transferredItemId = await TransferItem(itemId, roomId, userId, ItemId.NoItem, 0, new PropertySet { }, delProps);

                        // Also: dont wait to cleanup state, just ignore the presence-unavailable
                        RemoveRoomItem(roomId, itemId);
                    }
                }
            }
        }

        async Task SendPresenceAvailable(RoomItem roomItem, long x)
        {
            var roomId = roomItem.RoomId;
            long itemId = roomItem.ItemId;

            var props = await Inventory(roomId).GetItemProperties(itemId, new PidList { Pid.Name, Pid.Label, Pid.AnimationsUrl, Pid.Image100Url });

            var name = props.GetString(Pid.Name);
            if (string.IsNullOrEmpty(name)) { name = props.GetString(Pid.Label); }
            if (string.IsNullOrEmpty(name)) { name = $"Item-{itemId}"; }

            var roomItemJid = new RoomItemJid(roomId, itemId, name);

            var animationsUrl = props.GetString(Pid.AnimationsUrl);
            animationsUrl = PropertyFilter.Url(animationsUrl);
            var imageUrl = string.IsNullOrEmpty(animationsUrl) ? props.GetString(Pid.Image100Url) : "";
            imageUrl = PropertyFilter.Url(imageUrl);

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
            var name_XmlEncoded = WebUtility.HtmlEncode(name);
            var animationsUrl_XmlEncoded = string.IsNullOrEmpty(animationsUrl) ? "" : WebUtility.HtmlEncode(animationsUrl);
            var x_XmlEncoded = (x == 0) ? "" : WebUtility.HtmlEncode(x.ToString(CultureInfo.InvariantCulture));
            var imageUrl_XmlEncoded = string.IsNullOrEmpty(imageUrl) ? "" : WebUtility.HtmlEncode(imageUrl);
            var identitySrc_XmlEncoded = WebUtility.HtmlEncode(identitySrc);
            var identityDigest_XmlEncoded = WebUtility.HtmlEncode(identityDigest);
            var identityJid_XmlEncoded = WebUtility.HtmlEncode(identityJid);

            var animationsUrl_Attribute = $"animationsUrl='{animationsUrl_XmlEncoded}'";
            var imageUrl_Attribute = $"imageUrl='{imageUrl_XmlEncoded}'";
            var position_Node = $"<position x='{x_XmlEncoded}' />";

            Log.Info($"Rez {roomItemJid.Resource} {roomId} {itemId}", nameof(SendPresenceAvailable));

            if (_conn != null) {
                _conn.Send(
@$"<presence to='{to_XmlEncoded}' from='{from_XmlEncoded}'>
    <x xmlns='vp:props' nickname='{name_XmlEncoded}' {(string.IsNullOrEmpty(animationsUrl) ? "" : animationsUrl_Attribute)} {(string.IsNullOrEmpty(imageUrl) ? "" : imageUrl_Attribute)} />
    <x xmlns='firebat:user:identity' jid='{identityJid_XmlEncoded}' src='{identitySrc_XmlEncoded}' digest='{identityDigest_XmlEncoded}' />
    <x xmlns='firebat:avatar:state'>{position_Node}</x>
    <x xmlns='http://jabber.org/protocol/muc'><history seconds='0' maxchars='0' maxstanzas='0' /></x>
</presence>"
                );

                roomItem.Resource = roomItemJid.Resource;
            }
        }

        async Task SendPresenceUnvailable(RoomItem roomItem)
        {
            var roomId = roomItem.RoomId;
            long itemId = roomItem.ItemId;

            var to = $"{roomId}/{roomItem.Resource}";
            var from = $"{itemId}@{_componentDomain}/backend";

            var to_XmlEncoded = WebUtility.HtmlEncode(to);
            var from_XmlEncoded = WebUtility.HtmlEncode(from);

            Log.Info($"Derez {roomItem.Resource} {roomId} {itemId}", nameof(SendPresenceAvailable));

            _conn?.Send($"<presence to='{to_XmlEncoded}' from='{from_XmlEncoded}' type='unavailable' />");

            roomItem.State = RoomItem.RezState.Derezzing;

            await Task.CompletedTask;
        }

        #endregion

        #region Internal

        public async Task<long> TransferItem(long id, string sourceInventory, string destInventory, long containerId, long slot, PropertySet setProperties, PidList removeProperties)
        {
            var source = Inventory(sourceInventory);
            var dest = Inventory(destInventory);
            var sourceId = id;
            var destId = ItemId.NoItem;

            try {
                var transfer = await source.BeginItemTransfer(sourceId);
                if (transfer.Count == 0) {
                    throw new Exception("BeginItemTransfer: no data");
                }

                var map = await dest.ReceiveItemTransfer(id, containerId, slot, transfer, setProperties, removeProperties);
                destId = map[sourceId];

                await dest.EndItemTransfer(destId);

                await source.EndItemTransfer(sourceId);
            } catch (Exception ex) {
                if (destId != ItemId.NoItem) {
                    await dest.CancelItemTransfer(destId);
                }

                await source.CancelItemTransfer(sourceId);
                throw ex;
            }

            return destId;
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