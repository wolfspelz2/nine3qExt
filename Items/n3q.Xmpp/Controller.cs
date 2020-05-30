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
    public class Controller : IAsyncObserver<ItemUpdate>
    {
        readonly string _componentHost;
        readonly string _componentDomain;
        readonly int _componentPort;
        readonly string _componentSecret;

        readonly IClusterClient _clusterClient;
        StreamSubscriptionHandle<ItemUpdate> _subscriptionHandle;

        readonly object _mutex = new object();
        readonly Dictionary<string, ManagedRoom> _rooms = new Dictionary<string, ManagedRoom>();
        readonly Dictionary<string, RoomItem> _items = new Dictionary<string, RoomItem>();

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
            _subscriptionHandle = await ItemUpdateStream.SubscribeAsync(this);

            StartConnectionNewThread();
        }

        public async Task Shutdown()
        {
            await _subscriptionHandle.UnsubscribeAsync();
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
                var streamId = ItemService.StreamGuid;
                var streamNamespace = ItemService.StreamNamespace;
                var stream = streamProvider.GetStream<ItemUpdate>(streamId, streamNamespace);
                return stream;
            }
        }

        IItem GetItem(string roomId) => _clusterClient.GetGrain<IItem>(roomId);
        IWorker GetWorker() => _clusterClient.GetGrain<IWorker>(Guid.Empty);

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

                _ = managedRoom.Items.TryGetValue(itemId, out roomItem);
                if (roomItem == null) {
                    roomItem = new RoomItem(roomId, itemId);

                    managedRoom.Items.Add(itemId, roomItem);
                    _items.Add(itemId, roomItem);
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
                    _ = managedRoom.Items.TryGetValue(itemId, out roomItem);
                }
            }

            return roomItem;
        }

        RoomItem GetRoomItem(string itemId)
        {
            var roomItem = (RoomItem)null;

            lock (_mutex) {
                _ = _items.TryGetValue(itemId, out roomItem);
            }

            return roomItem;
        }

        bool IsManagedRoom(string roomId)
        {
            lock (_mutex) {
                return _rooms.ContainsKey(roomId);
            }
        }

        void RemoveRoomItem(string roomId, string itemId)
        {
            lock (_mutex) {
                if (_rooms.TryGetValue(itemId, out var managedRoom)) {
                    if (managedRoom.Items.ContainsKey(itemId)) {
                        managedRoom.Items.Remove(itemId);
                        if (managedRoom.Items.Count == 0) {

                            _rooms.Remove(roomId);
                            _items.Remove(itemId);
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
                    Log.Info($"Joined room {roomId} {itemId}", nameof(Connection_OnPresenceAvailable));
                    roomItem.State = RoomItem.RezState.Rezzed;
                    await GetWorker().Run(itemId, Pid.RezableAspect, nameof(Rezable.OnRezzed));
                }
            }

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
//                // Just in case, should already be removed after sending presence-unavailable
                RemoveRoomItem(roomId, itemId);
            }

            await Task.CompletedTask;
        }

        async Task Connection_OnDropItem(XmppMessage message)
        {
            var userId = message.Cmd.ContainsKey("user") ? message.Cmd["user"] : "";
            var itemId = message.Cmd.ContainsKey("item") ? message.Cmd["item"] : "";
            var roomId = message.Cmd.ContainsKey("room") ? message.Cmd["room"] : "";
            var hasX = long.TryParse(message.Cmd.ContainsKey("x") ? message.Cmd["x"] : "", out long posX);
            var destinationUrl = message.Cmd.ContainsKey("destination") ? message.Cmd["destination"] : "";

            if (Has.Value(userId) && Has.Value(itemId) && Has.Value(roomId) && hasX) {
                Log.Info($"Drop {roomId} {itemId}");
                await GetWorker().Run(itemId, Pid.RezableAspect, nameof(Rezable.Rez), new PropertySet { [Pid.RezableRoom] = roomId, [Pid.RezableX] = posX });
                //await OnItemAddedToRoom(roomId, itemId);
            }
        }

        async Task Connection_OnPickupItem(XmppMessage message)
        {
            var userId = message.Cmd.ContainsKey("user") ? message.Cmd["user"] : "";
            var itemId = message.Cmd.ContainsKey("item") ? message.Cmd["item"] : "";
            var roomId = message.Cmd.ContainsKey("room") ? message.Cmd["room"] : "";

            if (Has.Value(userId) && Has.Value(itemId) && Has.Value(roomId)) {
                var roomItem = GetRoomItem(roomId, itemId);
                if (roomItem != null) {
                    if (roomItem.State != RoomItem.RezState.Rezzed) {
                        Log.Warning($"Unexpected message-cmd-pickupItem: room={roomId} item={itemId}");
                        throw new SurfaceException(roomId, itemId, SurfaceNotification.Fact.NotDerezzed, SurfaceNotification.Reason.ItemIsNotRezzed);
                    } else {
                        Log.Info($"Pickup {roomId} {itemId}", nameof(Connection_OnPickupItem));
                        await GetWorker().Run(itemId, Pid.RezableAspect, nameof(Rezable.Derez), new PropertySet { [Pid.RezableUser] = roomId });
                        await OnItemRemovedFromRoom(roomItem);

                        // Also: don't wait to cleanup state, just ignore the presence-unavailable
                        RemoveRoomItem(roomId, itemId);
                    }
                }
            }

            await Task.CompletedTask;
        }

        async Task SendPresenceAvailable(RoomItem roomItem)
        {
            var roomId = roomItem.RoomId;
            var itemId = roomItem.ItemId;

            var props = await GetItem(itemId).GetProperties(PidSet.Public);

            var name = props.GetString(Pid.Name);
            if (string.IsNullOrEmpty(name)) { name = props.Get(Pid.Label); }
            if (string.IsNullOrEmpty(name)) { name = $"Item-{itemId}"; }

            var x = props.GetInt(Pid.RezableX);

            var roomItemJid = new RoomItemJid(roomId, itemId, name);

            var animationsUrl = props.GetString(Pid.AnimationsUrl);
            if (!string.IsNullOrEmpty(animationsUrl)) {
                if (props.ContainsKey(Pid.Image100Url)) {
                    props.Delete(Pid.Image100Url);
                }
            }

            var to = roomItemJid.Full;
            var from = $"{itemId}@{_componentDomain}/backend";
            var identityJid = $"{itemId}@{_componentDomain}";
            var identityDigest = Math.Abs(string.GetHashCode(name + animationsUrl, StringComparison.InvariantCulture)).ToString(CultureInfo.InvariantCulture);

            var name_UrlEncoded = WebUtility.UrlEncode(name);
            var animationsUrl_UrlEncoded = string.IsNullOrEmpty(animationsUrl) ? "" : WebUtility.UrlEncode(animationsUrl);
            var digest_UrlEncoded = WebUtility.UrlEncode(identityDigest);
            var identitySrc = $"https://avatar.weblin.sui.li/identity/?avatarUrl={animationsUrl_UrlEncoded}&nickname={name_UrlEncoded}&digest={digest_UrlEncoded}";

            var props_XmlEncoded = props.Select(pair => {
                var value = pair.Value.ToString();
                var key_XmlEncoded = WebUtility.HtmlEncode(pair.Key.ToString());
                var value_XmlEncoded = WebUtility.HtmlEncode(value);
                return new KeyValuePair<string, string>(key_XmlEncoded, value_XmlEncoded);
            });

            var to_XmlEncoded = WebUtility.HtmlEncode(to);
            var from_XmlEncoded = WebUtility.HtmlEncode(from);
            var x_XmlEncoded = (x == 0) ? "" : WebUtility.HtmlEncode(x.ToString(CultureInfo.InvariantCulture));
            var identitySrc_XmlEncoded = WebUtility.HtmlEncode(identitySrc);
            var identityDigest_XmlEncoded = WebUtility.HtmlEncode(identityDigest);
            var identityJid_XmlEncoded = WebUtility.HtmlEncode(identityJid);

            var props_XmlEncoded_All = "";
            foreach (var pair in props_XmlEncoded) {
                var attrName = pair.Key;
                //var attrNameCamelCased = Char.ToLowerInvariant(attrName[0]) + attrName.Substring(1);
                props_XmlEncoded_All += $" {attrName}='{pair.Value}'";
            }

            var position_Node = $"<position x='{x_XmlEncoded}' />";

            Log.Info($"Rez '{roomItemJid.Resource}' {roomId} {itemId}", nameof(SendPresenceAvailable));

            if (_conn != null) {
                _conn.Send(
                    $"<presence to='{to_XmlEncoded}' from='{from_XmlEncoded}'>"
                    + $"<x xmlns='vp:props' type='item' service='n3q' {props_XmlEncoded_All} /> "
                    + $"< x xmlns='firebat:user:identity' jid='{identityJid_XmlEncoded}' src='{identitySrc_XmlEncoded}' digest='{identityDigest_XmlEncoded}' /> "
                    + $"< x xmlns='firebat:avatar:state'>{position_Node}</x> "
                    + $"< x xmlns='http://jabber.org/protocol/muc'><history seconds='0' maxchars='0' maxstanzas='0' /></x> "
                    + $"</presence>"
                );

                roomItem.Resource = roomItemJid.Resource;
            }

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
                if (IsManagedRoom(update.ItemId)) {

                    foreach (var change in update.Changes) {
                        if (change.Pid == Pid.Contains && change.What == ItemChange.Mode.AddToList) {
                            var roomId = update.ItemId;
                            var itemId = change.Value;
                            await OnItemAddedToRoom(roomId, itemId);

                        } else if (change.Pid == Pid.Contains && change.What == ItemChange.Mode.RemoveFromList) {
                            var roomId = update.ItemId;
                            var itemId = change.Value;
                            var roomItem = GetRoomItem(roomId, itemId);
                            if (roomItem != null) {
                                await OnItemRemovedFromRoom(roomItem);
                            }
                        }
                    }

                } else {

                    var roomItem = GetRoomItem(update.ItemId);
                    if (roomItem != null) {
                        var atleastOneOfChangedPropertiesIsPublic = false;
                        foreach (var change in update.Changes) {
                            atleastOneOfChangedPropertiesIsPublic |= Property.GetDefinition(change.Pid).Access == Property.Access.Public;
                        }
                        if (atleastOneOfChangedPropertiesIsPublic) {
                            await OnPublicItemPropertyChanged(roomItem);
                        }

                    }

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

        #region Item events

        private async Task OnItemAddedToRoom(string roomId, string itemId)
        {
            var roomItem = AddRoomItem(roomId, itemId);

            await SendPresenceAvailable(roomItem);

            roomItem.State = RoomItem.RezState.Rezzing;
        }

        private async Task OnItemRemovedFromRoom(RoomItem roomItem)
        {
            await SendPresenceUnvailable(roomItem);
        }

        private async Task OnPublicItemPropertyChanged(RoomItem roomItem)
        {
            await SendPresenceAvailable(roomItem);
        }

        #endregion

    }

}