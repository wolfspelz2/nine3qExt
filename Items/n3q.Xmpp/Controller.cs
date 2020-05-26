using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Linq;
using Orleans;
using Orleans.Streams;
using n3q.Items;
using n3q.GrainInterfaces;
using n3q.Common;
using n3q.Tools;
using n3q.Aspects;

namespace XmppComponent
{
    internal partial class Controller : IAsyncObserver<ItemUpdate>
    {
        readonly string _componentHost;
        readonly string _componentDomain;
        readonly int _componentPort;
        readonly string _componentSecret;

        readonly IClusterClient _clusterClient;

        readonly object _mutex = new object();
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

        public async Task Start()
        {
            var handle = await ItemUpdateStream.SubscribeAsync(this);
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

        private IAsyncStream<ItemUpdate> ItemUpdateStream
        {
            get {
                var streamProvider = _clusterClient.GetStreamProvider(ItemService.StreamProvider);
                var streamId = ItemService.StreamGuidDefault;
                var streamNamespace = ItemService.StreamNamespaceDefault;
                var stream = streamProvider.GetStream<ItemUpdate>(streamId, streamNamespace);
                return stream;
            }
        }

        #endregion

        #region Management

        RoomItem AddRoomItem(string roomId, string itemId)
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

        RoomItem GetRoomItem(string roomId, string itemId)
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

        void RemoveRoomItem(string roomId, string itemId)
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

        #region OnConnection

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
            //var jid = new RoomItemJid(stanza.From);
            //var roomId = jid.Room;
            //var itemId = jid.Item;

            //var roomItem = GetRoomItem(roomId, itemId);
            //if (roomItem == null) {
            //    // Not my item
            //} else {
            //    if (roomItem.State != RoomItem.RezState.Rezzing) {
            //        Log.Warning($"Unexpected presence-available: room={roomId} item={itemId}", nameof(Connection_OnPresenceAvailable));
            //    } else {
            //        Log.Info($"Joined room {roomId} {itemId}", nameof(Connection_OnPresenceAvailable));
            //        roomItem.State = RoomItem.RezState.Rezzed;
            //        await Inventory(roomId).SetItemProperties(itemId, new PropertySet { [Pid.IsRezzed] = true });
            //    }
            //}

            await Task.CompletedTask;
        }

        async Task Connection_OnPresenceUnavailable(XmppPresence stanza)
        {
            var jid = new RoomItemJid(stanza.From);
            var roomId = jid.Room;
            var itemId = jid.Item;

            Log.Info($"Left room {roomId} {itemId}", nameof(Connection_OnPresenceUnavailable));

            var roomItem = GetRoomItem(roomId, itemId);
            if (roomItem != null) {
                // Just in case, shuld already be removed after sending presence-unavailable
                RemoveRoomItem(roomId, itemId);
            }

            await Task.CompletedTask;
        }

        Item GetItem(string roomId) { return new Item(_clusterClient, roomId); }

        async Task Connection_OnDropItem(XmppMessage message)
        {
            var userId = message.Cmd.ContainsKey("user") ? message.Cmd["user"] : "";
            var itemId = message.Cmd.ContainsKey("item") ? message.Cmd["item"] : "";
            var roomId = message.Cmd.ContainsKey("room") ? message.Cmd["room"] : "";
            var hasX = long.TryParse(message.Cmd.ContainsKey("x") ? message.Cmd["x"] : "", out long posX);
            var destinationUrl = message.Cmd.ContainsKey("destination") ? message.Cmd["destination"] : "";

            if (Has.Value(userId) && Has.Value(itemId) && Has.Value(roomId) && hasX) {
                //itemId = await TestPrepareItemForDrop(userId, roomId);

                Log.Info($"Drop {roomId} {itemId}");

                var room = GetItem(roomId);
                var item = GetItem(itemId);

                await item.AsRezable().AssertAspect(() => throw new SurfaceException(userId, itemId, SurfaceNotification.Fact.NotRezzed, SurfaceNotification.Reason.ItemNotRezable));
                await room.AsContainer().AddChild(item);

                var itemContainerId = await item.GetItemId(Pid.Container);
                if (itemContainerId != roomId) {
                    // ...
                }

                var roomItem = AddRoomItem(roomId, itemId);

                await SendPresenceAvailable(roomItem);

                //roomItem.State = RoomItem.RezState.Rezzing;
            }
        }

        async Task Connection_OnPickupItem(XmppMessage message)
        {
            //var userId = message.Cmd.ContainsKey("user") ? message.Cmd["user"] : "";
            //var itemId = ItemId.NoItem;
            //_ = long.TryParse(message.Cmd.ContainsKey("item") ? message.Cmd["item"] : "0", out itemId);
            //var roomId = message.Cmd.ContainsKey("room") ? message.Cmd["room"] : "";

            //if (!string.IsNullOrEmpty(userId) && itemId != ItemId.NoItem && !string.IsNullOrEmpty(roomId)) {

            //    var roomItem = GetRoomItem(roomId, itemId);
            //    if (roomItem != null) {
            //        if (roomItem.State != RoomItem.RezState.Rezzed) {
            //            Log.Warning($"Unexpected message-cmd-pickupItem: room={roomId} item={itemId}");
            //            throw new SurfaceException(roomId, itemId, SurfaceNotification.Fact.NotDerezzed, SurfaceNotification.Reason.ItemNotRezzed);
            //        } else {
            //            Log.Info($"Pickup {roomId} {itemId}", nameof(Connection_OnPickupItem));

            //            var props = await Inventory(roomId).GetItemProperties(itemId, new PidList { Pid.RezableAspect, Pid.IsRezzed });
            //            if (!props.GetBool(Pid.RezableAspect)) { throw new SurfaceException(roomId, itemId, SurfaceNotification.Fact.NotDerezzed, SurfaceNotification.Reason.ItemNotRezable); }
            //            if (!props.GetBool(Pid.IsRezzed)) { throw new SurfaceException(roomId, itemId, SurfaceNotification.Fact.NotDerezzed, SurfaceNotification.Reason.ItemNotRezzed); }

            //            await SendPresenceUnvailable(roomItem);

            //            // Transfer back before confirmation from xmpp room
            //            var transferredItemId = await TransferItem(itemId, roomId, userId, ItemId.NoItem, 0, new PropertySet { }, new PidList { Pid.RezzedX, Pid.IsRezzed });

            //            // Also: dont wait to cleanup state, just ignore the presence-unavailable
            //            RemoveRoomItem(roomId, itemId);
            //        }
            //    }
            //}

            await Task.CompletedTask;
        }

        async Task SendPresenceAvailable(RoomItem roomItem)
        {
            var roomId = roomItem.RoomId;
            var itemId = roomItem.ItemId;

            //var props = await Inventory(roomId).GetItemProperties(itemId, new PidList { Pid.Name, Pid.Label, Pid.AnimationsUrl, Pid.Image100Url, Pid.RezzedX });
            //var props = await Inventory(roomId).GetItemProperties(itemId, PidList.Public);
            var props = await GetItem(itemId).GetProperties(PidSet.Public);

        //    var name = props.GetString(Pid.Name);
        //    if (string.IsNullOrEmpty(name)) { name = props.GetString(Pid.Label); }
        //    if (string.IsNullOrEmpty(name)) { name = $"Item-{itemId}"; }

        //    var x = props.GetInt(Pid.RezzedX);
        //    props.Delete(Pid.RezzedX);

        //    var roomItemJid = new RoomItemJid(roomId, itemId, name);

        //    var animationsUrl = props.GetString(Pid.AnimationsUrl);
        //    if (!string.IsNullOrEmpty(animationsUrl)) {
        //        animationsUrl = PropertyFilter.Url(animationsUrl);
        //        if (props.ContainsKey(Pid.Image100Url)) {
        //            props.Delete(Pid.Image100Url);
        //        }
        //    }

        //    var to = roomItemJid.Full;
        //    var from = $"{itemId}@{_componentDomain}/backend";
        //    var identityJid = $"{itemId}@{_componentDomain}";
        //    var identityDigest = Math.Abs(string.GetHashCode(name + animationsUrl, StringComparison.InvariantCulture)).ToString(CultureInfo.InvariantCulture);

        //    var name_UrlEncoded = WebUtility.UrlEncode(name);
        //    var animationsUrl_UrlEncoded = string.IsNullOrEmpty(animationsUrl) ? "" : WebUtility.UrlEncode(animationsUrl);
        //    var digest_UrlEncoded = WebUtility.UrlEncode(identityDigest);
        //    var identitySrc = $"https://avatar.weblin.sui.li/identity/?avatarUrl={animationsUrl_UrlEncoded}&nickname={name_UrlEncoded}&digest={digest_UrlEncoded}";

        //    var props_XmlEncoded = props.Select(pair => {
        //        var value = Property.ToString(pair.Key, pair.Value);
        //        var propDef = Property.Get(pair.Key);
        //        if (propDef.Type == Property.Type.String && (propDef.Use == Property.Use.Url || propDef.Use == Property.Use.ImageUrl)) {
        //            value = PropertyFilter.Url(value);
        //        }
        //        var key_XmlEncoded = WebUtility.HtmlEncode(pair.Key.ToString());
        //        var value_XmlEncoded = WebUtility.HtmlEncode(value);
        //        return new KeyValuePair<string, string>(key_XmlEncoded, value_XmlEncoded);
        //    });

        //    var to_XmlEncoded = WebUtility.HtmlEncode(to);
        //    var from_XmlEncoded = WebUtility.HtmlEncode(from);
        //    var x_XmlEncoded = (x == 0) ? "" : WebUtility.HtmlEncode(x.ToString(CultureInfo.InvariantCulture));
        //    var identitySrc_XmlEncoded = WebUtility.HtmlEncode(identitySrc);
        //    var identityDigest_XmlEncoded = WebUtility.HtmlEncode(identityDigest);
        //    var identityJid_XmlEncoded = WebUtility.HtmlEncode(identityJid);

        //    var props_XmlEncoded_All = "";
        //    foreach (var pair in props_XmlEncoded) {
        //        var attrName = pair.Key;
        //        //var attrNameCamelCased = Char.ToLowerInvariant(attrName[0]) + attrName.Substring(1);
        //        props_XmlEncoded_All += $" {attrName}='{pair.Value}'";
        //    }

        //    var position_Node = $"<position x='{x_XmlEncoded}' />";

        //    Log.Info($"Rez '{roomItemJid.Resource}' {roomId} {itemId}", nameof(SendPresenceAvailable));

        //    if (_conn != null) {
        //        _conn.Send(
        //@$"<presence to='{to_XmlEncoded}' from='{from_XmlEncoded}'>
        //    <x xmlns='vp:props' type='item' {props_XmlEncoded_All} />
        //    <x xmlns='firebat:user:identity' jid='{identityJid_XmlEncoded}' src='{identitySrc_XmlEncoded}' digest='{identityDigest_XmlEncoded}' />
        //    <x xmlns='firebat:avatar:state'>{position_Node}</x>
        //    <x xmlns='http://jabber.org/protocol/muc'><history seconds='0' maxchars='0' maxstanzas='0' /></x>
        //    </presence>"
        //        );

        //        roomItem.Resource = roomItemJid.Resource;
        //    }

            await Task.CompletedTask;
        }

        async Task SendPresenceUnvailable(RoomItem roomItem)
        {
            var roomId = roomItem.RoomId;
            var itemId = roomItem.ItemId;
            var roomResource = roomItem.Resource;

            var to = $"{roomId}/{roomResource}";
            var from = $"{itemId}@{_componentDomain}/backend";

            var to_XmlEncoded = WebUtility.HtmlEncode(to);
            var from_XmlEncoded = WebUtility.HtmlEncode(from);

            Log.Info($"Derez '{roomResource}' {roomId} {itemId}", nameof(SendPresenceAvailable));

            _conn?.Send($"<presence to='{to_XmlEncoded}' from='{from_XmlEncoded}' type='unavailable' />");

            roomItem.State = RoomItem.RezState.Derezzing;

            await Task.CompletedTask;
        }

        #endregion

        #region Stream

        public async Task OnNextAsync(ItemUpdate itemUpdate, StreamSequenceToken token = null)
        {
            await OnItemUpdate(itemUpdate);
        }

        public async Task OnItemUpdate(ItemUpdate update)
        {
            //if (update.What == ItemUpdate.Mode.Removed) {
            //    // if room/update.Id is ManagedRoom
            //    // presence-unavailable?
            //    return;
            //}

            //if (update.What == ItemUpdate.Mode.Added) {
            //    // if room/update.Id is ManagedRoom
            //    // presence-unavailable?
            //    return;
            //}

            //var roomId = update.InventoryId;
            //var itemId = update.Id;

            //var roomItem = GetRoomItem(roomId, itemId);
            //if (roomItem != null) {

            //    var atleastOneOfChangedPropertiesIsPublic = false;
            //    if (update.Pids != null) {
            //        foreach (var pid in update.Pids) {
            //            if (Property.Get(pid).Access == Property.Access.Public) {
            //                atleastOneOfChangedPropertiesIsPublic = true;
            //                break;
            //            }
            //        }
            //    }

            //    if (atleastOneOfChangedPropertiesIsPublic) {
            //        await SendPresenceAvailable(roomItem);
            //    }
            //}
            await Task.CompletedTask;
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

        #region Internal

        //public async Task<long> TransferItem(long id, string sourceInventory, string destInventory, long containerId, long slot, PropertySet setProperties, PidList removeProperties)
        //{
        //    var source = Inventory(sourceInventory);
        //    var dest = Inventory(destInventory);
        //    var sourceId = id;
        //    var destId = ItemId.NoItem;

        //    try {
        //        var transfer = await source.BeginItemTransfer(sourceId);
        //        if (transfer.Count == 0) {
        //            throw new Exception("BeginItemTransfer: no data");
        //        }

        //        var map = await dest.ReceiveItemTransfer(id, containerId, slot, transfer, setProperties, removeProperties);
        //        destId = map[sourceId];

        //        await dest.EndItemTransfer(destId);

        //        await source.EndItemTransfer(sourceId);
        //    } catch (Exception ex) {
        //        if (destId != ItemId.NoItem) {
        //            await dest.CancelItemTransfer(destId);
        //        }

        //        await source.CancelItemTransfer(sourceId);
        //        throw ex;
        //    }

        //    return destId;
        //}

        #endregion

    }

}