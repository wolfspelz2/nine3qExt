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
    public partial class Controller : IAsyncObserver<ItemUpdate>
    {
        readonly string _componentHost;
        readonly string _componentDomain;
        readonly int _componentPort;
        readonly string _componentSecret;
        readonly string _roomStorageId;
        readonly string _configStorageId;

        readonly IClusterClient _clusterClient;
        StreamSubscriptionHandle<ItemUpdate> _subscriptionHandle;

        readonly object _mutex = new object();
        readonly Dictionary<string, Room> _rooms = new Dictionary<string, Room>();
        readonly Dictionary<string, RoomItem> _roomItems = new Dictionary<string, RoomItem>();
        readonly Dictionary<string, Inventory> _inventories = new Dictionary<string, Inventory>();
        readonly Dictionary<string, Inventory> _inventoryItems = new Dictionary<string, Inventory>();

        private Connection _xmppConnection;
        private bool _clusterConnected;

        public Controller(IClusterClient clusterClient, string componentHost, string componentDomain, int componentPort, string componentSecret)
        {
            _clusterClient = clusterClient;
            _componentHost = componentHost;
            _componentDomain = componentDomain;
            _componentPort = componentPort;
            _componentSecret = componentSecret;

            _roomStorageId = "Xmpp-Rooms-jnjnhbgtf7tugzhjktr5ru-" + componentDomain;
            _configStorageId = "Xmpp-Config-rtfgzuh65rvgbz8hlklkj-" + componentDomain;
        }

        public async Task Start()
        {
            await SubscribeItemUpdateStream();
            _clusterConnected = true;

            StartConnectionInNewThread();
        }

        public void OnClusterDisconnect()
        {
            _clusterConnected = false;
        }

        public async Task OnClusterReconnect()
        {
            await SubscribeItemUpdateStream();
            _clusterConnected = true;
        }

        private async Task SubscribeItemUpdateStream()
        {
            var handles = await ItemUpdateStream.GetAllSubscriptionHandles();
            if (handles.Count == 0) {
                if (_subscriptionHandle == null) {
                    _subscriptionHandle = await ItemUpdateStream.SubscribeAsync(this);
                } else {
                    await _subscriptionHandle.ResumeAsync(this);
                }
            } else {
                foreach (var handle in handles) {
                    await handle.ResumeAsync(this);
                }
            }
            //_subscriptionHandle = await ItemUpdateStream.SubscribeAsync(this);
        }

        async Task PopulateRooms()
        {
            var roomList = await MakeItemStub(_roomStorageId).GetItemIdList(Pid.XmppRoomList);
            foreach (var roomId in roomList) {
                await PopulateRoom(roomId);
            }
        }

        async Task PopulateRoom(string roomId)
        {
            var itemList = await MakeItemStub(roomId).GetItemIdList(Pid.Contains);
            foreach (var itemId in itemList) {
                var roomItem = await AddRoomItem(roomId, itemId, updatePersistentState: false);
                roomItem.State = RoomItem.RezState.Rezzing;
                await SendRoomItemPresenceAvailable(roomId, itemId);
            }
        }

        async Task SendAllItemPresenceToInventorySubscriber(string inventoryItemId, string subscriberJid, string clientJid)
        {
            var fromJid = new XmppJid(subscriberJid);

            var itemList = await MakeItemStub(inventoryItemId).GetItemIdList(Pid.Contains);
            foreach (var itemId in itemList) {
                var itemFrom = fromJid.Base + "/" + itemId;
                await SendInventoryItemPresenceAvailable(itemId, itemFrom, clientJid);
            }
        }

        public async Task Shutdown()
        {
            await _subscriptionHandle.UnsubscribeAsync();
        }

        internal void Send(string line)
        {
            if (_xmppConnection != null) {
                _xmppConnection.Send(line);
            }
        }

        void StartConnectionInNewThread()
        {
            Task.Run(async () => {
                _xmppConnection = new Connection(
                    _componentHost,
                    _componentDomain,
                    _componentPort,
                    _componentSecret,
                    async conn => { await Connection_OnStarted(conn); },
                    async cmd => { await Connection_OnMessage(cmd); },
                    async cmd => { await Connection_OnPresence(cmd); },
                    conn => { Connection_OnClosed(conn); }
                    );

                await _xmppConnection.Run();
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

        ItemStub MakeItemStub(string itemId)
        {
            var itemClient = new OrleansClusterClient(_clusterClient, itemId);
            var itemStub = new ItemStub(itemClient);
            return itemStub;
        }

        IWorker GetIWorker() => _clusterClient.GetGrain<IWorker>(Guid.Empty);

        #endregion

        #region Management

        bool IsInventorySubscriber(string userId, string clientJid)
        {
            lock (_mutex) {
                if (_inventories.TryGetValue(userId, out var inv)) {
                    return inv.Subscribers.ContainsKey(clientJid);
                }
            }
            return false;
        }

        bool IsInventoryByUserId(string userId)
        {
            return GetInventoryByUserId(userId) != null;
        }

        bool IsInventoryByItemId(string inventoryItemId)
        {
            return GetInventoryByItemId(inventoryItemId) != null;
        }

        Inventory GetInventoryByUserId(string userId)
        {
            lock (_mutex) {
                _ = _inventories.TryGetValue(userId, out var inv);
                return inv;
            }
        }

        Inventory GetInventoryByItemId(string inventoryItemId)
        {
            lock (_mutex) {
                _ = _inventoryItems.TryGetValue(inventoryItemId, out var inv);
                return inv;
            }
        }

        InventorySubscriber AddInventorySubscriber(string userId, string inventoryItemId, string participantJid, string clientJid)
        {
            var inventorySubscriber = (InventorySubscriber)null;

            lock (_mutex) {
                var inv = (Inventory)null;
                if (!_inventories.ContainsKey(userId)) {
                    inv = new Inventory(userId, inventoryItemId, participantJid);
                    _inventories[userId] = inv;
                    _inventoryItems[inventoryItemId] = inv;
                } else {
                    inv = _inventories[userId];
                }
                if (!inv.Subscribers.ContainsKey(participantJid)) {
                    inventorySubscriber = new InventorySubscriber(userId, participantJid, clientJid);
                    inv.Subscribers.Add(participantJid, inventorySubscriber);
                }
            }

            return inventorySubscriber;
        }

        void RemoveInventorySubscriber(string userId, string participantJid)
        {
            lock (_mutex) {
                if (_inventories.TryGetValue(userId, out var inv)) {
                    if (inv.Subscribers.ContainsKey(participantJid)) {
                        inv.Subscribers.Remove(participantJid);
                        if (inv.Subscribers.Count == 0) {
                            _inventoryItems.Remove(inv.InventoryItemId);
                            _inventories.Remove(userId);
                        }
                    }
                }
            }
        }

        async Task<RoomItem> AddRoomItem(string roomId, string itemId, bool updatePersistentState = true)
        {
            var roomItem = (RoomItem)null;
            var roomCreated = false;

            lock (_mutex) {
                var managedRoom = (Room)null;
                if (!_rooms.ContainsKey(roomId)) {
                    _rooms[roomId] = new Room(roomId);
                    roomCreated = true;
                }
                managedRoom = _rooms[roomId];

                _ = managedRoom.Items.TryGetValue(itemId, out roomItem);
                if (roomItem == null) {
                    roomItem = new RoomItem(roomId, itemId);

                    managedRoom.Items[itemId] = roomItem;
                    _roomItems[itemId] = roomItem;
                }
            }

            if (roomCreated && updatePersistentState) {
                await MakeItemStub(_roomStorageId).WithoutTransaction(async self => await self.AddToList(Pid.XmppRoomList, roomId));
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
                _ = _roomItems.TryGetValue(itemId, out roomItem);
            }

            return roomItem;
        }

        bool IsRoom(string roomId)
        {
            lock (_mutex) {
                return _rooms.ContainsKey(roomId);
            }
        }

        async Task RemoveRoomItem(string roomId, string itemId)
        {
            var roomDeleted = false;

            lock (_mutex) {
                if (_rooms.TryGetValue(roomId, out var managedRoom)) {
                    if (managedRoom.Items.ContainsKey(itemId)) {
                        managedRoom.Items.Remove(itemId);
                        _roomItems.Remove(itemId);
                        if (managedRoom.Items.Count == 0) {
                            _rooms.Remove(roomId);
                            roomDeleted = true;
                        }
                    }
                }
            }

            if (roomDeleted) {
                await MakeItemStub(_roomStorageId).WithoutTransaction(async self => await self.RemoveFromList(Pid.XmppRoomList, roomId));
            }
        }

        #endregion

        #region OnConnection

        async Task Connection_OnStarted(Connection conn)
        {
            Log.Info("Component connected to XMPP server");
            await PopulateRooms();
        }

        void Connection_OnClosed(Connection conn)
        {
            _xmppConnection = null;

            Thread.Sleep(3000);

            StartConnectionInNewThread();
        }

        async Task Connection_OnMessage(XmppMessage stanza)
        {
            try {
                if (!_clusterConnected) { throw new SurfaceException(SurfaceNotification.Fact.NotExecuted, SurfaceNotification.Reason.ServiceUnavailable); }

                switch (stanza.MessageType) {
                    case XmppMessageType.Normal: await Connection_OnNormalMessage(stanza); break;
                    case XmppMessageType.Groupchat: await Connection_OnGroupchatMessage(stanza); break;
                    case XmppMessageType.PrivateChat: await Connection_OnPrivateMessage(stanza); break;
                }
            } catch (SurfaceException ex) {
                SendExceptionResponseMessage(stanza, ex);
                //Log.Error(ex);
            } catch (Exception ex) {
                Log.Error(ex);
            }
        }

        /*
        <message to='fjri7apdci8uv2f4rj29bu49dg@xmpp.weblin.sui.li/V6O4rrkgmwEkWD0' from='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org' xmlns='jabber:client' type='error'>
            <body>Hallo</body>
            <error type='modify' code='406'><not-acceptable xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>
                <text xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'>Improper message type</text>
            </error>
        </message>
        */
        private void SendExceptionResponseMessage(XmppMessage stanza, SurfaceException ex)
        {
            var to = stanza.From;
            var from = $"{_componentDomain}";
            var id = stanza.Id;
            var body = ex.Message;

            var to_XmlEncoded = WebUtility.HtmlEncode(to);
            var from_XmlEncoded = WebUtility.HtmlEncode(from);
            var id_XmlEncoded = WebUtility.HtmlEncode(id);
            var body_XmlEncoded = WebUtility.HtmlEncode(body);

            _xmppConnection?.Send(
#pragma warning disable format
                  $"<message to='{to_XmlEncoded}' from='{from_XmlEncoded}' id='{id_XmlEncoded}' xmlns='jabber:client' type='error'>"
                +       $"<body>{body_XmlEncoded}</body>"
                +       $"<error type='wait' code='500'><internal-server-error xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>"
                +           $"<text xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'>Service not available</text>"
                +       $"</error>"
                + $"</message>"
#pragma warning restore format
                );
        }

        async Task Connection_OnPresence(XmppPresence stanza)
        {
            if (!_clusterConnected) { return; }

            try {
                switch (stanza.PresenceType) {
                    case XmppPresenceType.Available: await Connection_OnPresenceAvailable(stanza); break;
                    case XmppPresenceType.Unavailable: await Connection_OnPresenceUnavailable(stanza); break;
                    case XmppPresenceType.Error: await Connection_OnPresenceError(stanza); break;
                }
            } catch (Exception ex) {
                Log.Error(ex);
            }
        }

        async Task Connection_OnNormalMessage(XmppMessage stanza)
        {
            try {
                var method = stanza.Cmd.ContainsKey("method") ? stanza.Cmd["method"] : "";
                switch (method) {
                    case "itemAction": {
                        var user = stanza.Cmd.ContainsKey("user") ? stanza.Cmd["user"] : "";
                        var to = new XmppJid(stanza.To);
                        await Connection_OnItemAction(stanza, user, to.Resource);
                    }
                    break;
                    default: Log.Warning($"Unknown method={method}"); break;
                }

            } catch (Exception ex) {
                Log.Error(ex);
            }
        }

        async Task Connection_OnPrivateMessage(XmppMessage stanza)
        {
            try {
                var method = stanza.Cmd.ContainsKey("method") ? stanza.Cmd["method"] : "";
                switch (method) {
                    case "itemAction": {
                        var from = new XmppJid(stanza.From);
                        var to = new XmppJid(stanza.To);
                        if (IsRoom(from.Base)) {
                            await Connection_OnItemAction(stanza, "", to.User);
                        }
                        if (IsInventoryByUserId(to.User)) {
                            await Connection_OnItemAction(stanza, to.User, to.Resource);
                        }
                    }
                    break;
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

        // <- <presence to='user1@items.xmpp.dev.sui.li/hbsu6rtfzgasd' from='hbtzfgjhg@xmpp.weblin.sui.li/jhgjzgjuz' />

        // -> <presence to='hbtzfgjhg@xmpp.weblin.sui.li/jhgjzgjuz' from='user1@items.xmpp.dev.sui.li/CoffeeMachine1' />
        // -> <presence to='hbtzfgjhg@xmpp.weblin.sui.li/jhgjzgjuz' from='user1@items.xmpp.dev.sui.li/Script1' />
        // -> <presence to='hbtzfgjhg@xmpp.weblin.sui.li/jhgjzgjuz' from='user1@items.xmpp.dev.sui.li/hbsu6rtfzgasd' />

        async Task Connection_OnPresenceError(XmppPresence stanza)
        {
            Log.Warning($"{nameof(Connection_OnPresenceError)}: from={stanza.From} to={stanza.To}");

            var jid = new XmppJid(stanza.From);
            var roomId = jid.Base;
            var itemId = jid.Resource;

            var roomItem = GetRoomItem(roomId, itemId);
            if (roomItem != null) {
                if (roomItem.State == RoomItem.RezState.Rezzing) {
                    await GetIWorker().AspectAction(itemId, Pid.RezableAspect, nameof(Rezable.OnRezFailed));
                    await RemoveRoomItem(roomId, itemId);
                }
            }
            
            await Task.CompletedTask;
        }

        async Task Connection_OnPresenceAvailable(XmppPresence stanza)
        {
            var jid = new XmppJid(stanza.From);
            var roomId = jid.Base;
            var itemId = jid.Resource;

            var roomItem = GetRoomItem(roomId, itemId);
            if (roomItem != null) {
                if (roomItem.State == RoomItem.RezState.Rezzed) {
                    // Any of my items, already rezzed;
                } else if (roomItem.State == RoomItem.RezState.Rezzing) {
                    Log.Info($"Joined room {roomId} {itemId}");
                    roomItem.State = RoomItem.RezState.Rezzed;
                    await GetIWorker().AspectAction(itemId, Pid.RezableAspect, nameof(Rezable.OnRezzed));
                } else {
                    Log.Warning($"Unexpected presence-available: room={roomId} item={itemId} State={roomItem.State}", nameof(Connection_OnPresenceAvailable));
                }
            } else {

                // Maybe an inventory subscription
                var participantJid = stanza.To;
                var clientJid = stanza.From;
                var participantXmppJid = new XmppJid(participantJid);

                if (IsRoom(participantXmppJid.Base)) {
                    // Actually, a join or presence update from a client
                } else {
                    var userToken = participantXmppJid.User;
                    var inventoryItemId = await GetInventoryFromUserToken(userToken);
                    if (Has.Value(inventoryItemId)) {

                        if (!IsInventorySubscriber(userToken, participantJid)) {
                            AddInventorySubscriber(userToken, inventoryItemId, participantJid, clientJid);
                            await SendAllItemPresenceToInventorySubscriber(inventoryItemId, participantJid, clientJid);
                            await SendSubscriberPresenceAvailableConfirmation(participantJid, clientJid);
                        }

                    }
                }
            }

            await Task.CompletedTask;
        }

        async Task<string> GetInventoryFromUserToken(string userToken)
        {
            await Task.CompletedTask;
            var inventoryItemId = userToken == "random-user-token-jhg2fu7kjjl4koi8tgi" ? "random-user-inventory-576gzfezgfr54u6l9" : "";
            return inventoryItemId;
        }

        async Task Connection_OnPresenceUnavailable(XmppPresence stanza)
        {
            var jid = new XmppJid(stanza.From);
            var roomId = jid.Base;
            var itemId = jid.Resource;

            var roomItem = GetRoomItem(roomId, itemId);
            if (roomItem != null) {
                Log.Info($"Left room {roomId} {itemId}");

                // Just in case. Should already be removed after sending presence-unavailable
                await RemoveRoomItem(roomId, itemId);

            } else {

                // Maybe an inventory subscription
                var participantJid = stanza.To;
                var clientJid = stanza.From;
                var participantXmppJid = new XmppJid(participantJid);

                if (IsRoom(participantXmppJid.Base)) {
                    // Actually, a room leave from a client
                } else {

                    var userToken = new XmppJid(stanza.To).User;
                    if (IsInventorySubscriber(userToken, participantJid)) {
                        RemoveInventorySubscriber(userToken, participantJid);
                        await SendSubscriberPresenceUnavailableConfirmation(participantJid, clientJid);
                    }

                }
            }

            await Task.CompletedTask;
        }

        async Task Connection_OnItemAction(XmppMessage message, string user, string itemId)
        {
            var action = "";
            var args = new Dictionary<string, string>();
            foreach (var pair in message.Cmd) {
                switch (pair.Key) {
                    case "method": break;
                    case "xmlns": break;
                    case "action": action = pair.Value; break;
                    case "user": user = pair.Value; break;
                    default: args[pair.Key] = pair.Value; break;
                }
            }

            Log.Info($"ItemAction user={user} item={itemId} action={action}");

            switch (action) {

                case nameof(Rezable.Action.Rez): {
                    var inventoryItemId = await GetInventoryFromUserToken(user);
                    if (Has.Value(inventoryItemId) && Has.Value(itemId)) {
                        if (await MakeItemStub(itemId).GetBool(Pid.RezableAspect)) {
                            var roomId = message.Cmd.ContainsKey("to") ? message.Cmd["to"] : "";
                            if (Has.Value(roomId)) {
                                _ = await AddRoomItem(roomId, itemId);

                                var room = MakeItemStub(roomId);
                                if (!await room.Get(Pid.ContainerAspect)) {
                                    await room.WithTransaction(async self => { await self.Set(Pid.ContainerAspect, true); });
                                }

                                await GetIWorker().AspectAction(itemId, Pid.RezableAspect, nameof(Rezable.Action.Rez), new PropertySet {
                                    [Pid.RezableRezTo] = roomId,
                                    [Pid.RezableRezX] = message.Get("x", 200)
                                });

                            }
                        }
                    }
                }
                break;

                case nameof(Rezable.Action.Derez): {
                    var inventoryItemId = await GetInventoryFromUserToken(user);
                    if (Has.Value(inventoryItemId) && Has.Value(itemId)) {

                        if (await MakeItemStub(itemId).GetBool(Pid.RezableAspect)) {
                            var roomItem = GetRoomItem(itemId);
                            if (roomItem != null) {

                                await GetIWorker().AspectAction(itemId, Pid.RezableAspect, nameof(Rezable.Action.Derez), new PropertySet {
                                    [Pid.RezableDerezTo] = inventoryItemId,
                                    [Pid.RezableDerezX] = message.Get("x", -1),
                                    [Pid.RezableDerezY] = message.Get("y", -1)
                                });
                            }

                        }
                    }
                }
                break;

                case nameof(Movable.Action.MoveTo): {
                    var inventoryItemId = await GetInventoryFromUserToken(user);
                    if (Has.Value(inventoryItemId) && Has.Value(itemId)) {

                        var roomItem = GetRoomItem(itemId);
                        if (roomItem != null) {
                            await GetIWorker().AspectAction(itemId, Pid.MovableAspect, nameof(Movable.Action.MoveTo), new PropertySet {
                                [Pid.MovableMoveToX] = message.Get("x", -1),
                            });
                        }

                    }
                }
                break;

                case nameof(n3q.Aspects.Inventory.Action.SetItemCoordinates): {
                    var inventoryItemId = await GetInventoryFromUserToken(user);
                    if (Has.Value(inventoryItemId) && Has.Value(itemId)) {

                        if (await MakeItemStub(inventoryItemId).GetBool(Pid.InventoryAspect)) {
                            await GetIWorker().AspectAction(inventoryItemId, Pid.InventoryAspect, nameof(n3q.Aspects.Inventory.Action.SetItemCoordinates), new PropertySet {
                                [Pid.InventorySetItemCoordinatesItem] = itemId,
                                [Pid.InventorySetItemCoordinatesX] = message.Get("x", -1),
                                [Pid.InventorySetItemCoordinatesY] = message.Get("y", -1)
                            });

                        }
                    }
                }
                break;

                //case nameof(n3q.Aspects.Settings.Action.SetInventoryCoordinates): {
                //    var inventoryItemId = await GetInventoryFromUserToken(user);
                //    if (Has.Value(inventoryItemId) && Has.Value(itemId)) {

                //        if (await MakeItemStub(itemId).GetBool(Pid.SettingsAspect)) {
                //            await GetIWorker().AspectAction(itemId, Pid.SettingsAspect, nameof(n3q.Aspects.Settings.Action.SetInventoryCoordinates), new PropertySet {
                //                [Pid.SettingsSetInventoryCoordinatesLeft] = message.Get("left", -1),
                //                [Pid.SettingsSetInventoryCoordinatesBottom] = message.Get("bottom", -1),
                //                [Pid.SettingsSetInventoryCoordinatesWidth] = message.Get("width", -1),
                //                [Pid.SettingsSetInventoryCoordinatesHeight] = message.Get("height", -1)
                //            });

                //        }
                //    }
                //}
                //break;
            }

            //var executed = await GetWorker().ItemAction(userId, itemId, actionName, args);
            //if (executed.Count > 0) {
            //    Log.Info($"ItemAction executed {string.Join(" ", executed.Select(pair => pair.Key + "." + pair.Value))} {string.Join(" ", args.Select(pair => pair.Key + "=" + pair.Value))}");
            //}
        }

        #endregion

        #region Stream

        public async Task OnNextAsync(ItemUpdate itemUpdate, StreamSequenceToken token = null)
        {
            await OnItemUpdate(itemUpdate);
        }

        public async Task OnItemUpdate(ItemUpdate update)
        {
            if (false) {
            } else if (IsRoom(update.ItemId)) {
                await OnItemUpdateRoom(update);

            } else if (IsRoom(update.ParentId)) {
                await OnItemUpdateRoomItem(update);

            } else if (IsInventoryByItemId(update.ItemId)) {
                await OnItemUpdateInventory(update);

            } else if (IsInventoryByItemId(update.ParentId)) {
                await OnItemUpdateInventoryItem(update);

            }
        }

        private async Task OnItemUpdateInventoryItem(ItemUpdate update)
        {
            var itemId = update.ItemId;
            var inv = GetInventoryByItemId(update.ParentId);
            if (inv != null) {
                var itemFrom = new XmppJid(inv.ParticipantJid).Base + "/" + itemId;
                foreach (var pair in inv.Subscribers) {
                    var subscriber = pair.Value;
                    await SendInventoryItemPresenceAvailable(itemId, itemFrom, subscriber.ClientJid);
                }
            }
        }

        private async Task OnItemUpdateInventory(ItemUpdate update)
        {
            foreach (var change in update.Changes) {
                if (change.Pid == Pid.Contains && change.What == ItemChange.Mode.AddToList) {
                    var itemId = change.Value;
                    var inv = GetInventoryByItemId(update.ItemId);
                    if (inv != null) {
                        var itemFrom = new XmppJid(inv.ParticipantJid).Base + "/" + itemId;
                        foreach (var pair in inv.Subscribers) {
                            var subscriber = pair.Value;
                            await SendInventoryItemPresenceAvailable(itemId, itemFrom, subscriber.ClientJid);
                        }
                    }
                } else if (change.Pid == Pid.Contains && change.What == ItemChange.Mode.RemoveFromList) {
                    var itemId = change.Value;
                    var inv = GetInventoryByItemId(update.ItemId);
                    if (inv != null) {
                        var itemFrom = new XmppJid(inv.ParticipantJid).Base + "/" + itemId;
                        foreach (var pair in inv.Subscribers) {
                            var subscriber = pair.Value;
                            await SendInventoryItemPresenceUnavailable(itemFrom, subscriber.ClientJid);
                        }
                    }
                }
            }
        }

        private async Task OnItemUpdateRoomItem(ItemUpdate update)
        {
            var roomItem = GetRoomItem(update.ItemId);
            if (roomItem != null) {
                if (roomItem.State == RoomItem.RezState.Rezzed) {
                    var atleastOneOfChangedPropertiesIsPublic = false;
                    foreach (var change in update.Changes) {
                        atleastOneOfChangedPropertiesIsPublic |= Property.GetDefinition(change.Pid).Access == Property.Access.Public;
                    }
                    if (atleastOneOfChangedPropertiesIsPublic) {
                        await SendRoomItemPresenceAvailable(roomItem.RoomId, roomItem.ItemId);
                    }
                }
            }
        }

        private async Task OnItemUpdateRoom(ItemUpdate update)
        {
            foreach (var change in update.Changes) {
                if (change.Pid == Pid.Contains && change.What == ItemChange.Mode.AddToList) {
                    var roomId = update.ItemId;
                    var itemId = change.Value;
                    var roomItem = await AddRoomItem(roomId, itemId);
                    roomItem.State = RoomItem.RezState.Rezzing;
                    await SendRoomItemPresenceAvailable(roomId, itemId);

                } else if (change.Pid == Pid.Contains && change.What == ItemChange.Mode.RemoveFromList) {
                    var roomId = update.ItemId;
                    var itemId = change.Value;
                    var roomItem = GetRoomItem(roomId, itemId);
                    if (roomItem != null) {
                        await SendRoomItemPresenceUnvailable(roomItem.RoomId, roomItem.ItemId);
                        roomItem.State = RoomItem.RezState.Derezzing;

                        // Before presence-unavailable confirmation
                        await RemoveRoomItem(roomId, itemId);
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

        #region Send presence

        async Task SendSubscriberPresenceAvailableConfirmation(string from, string to)
        {
            if (_xmppConnection == null) { return; }

            var to_XmlEncoded = WebUtility.HtmlEncode(to);
            var from_XmlEncoded = WebUtility.HtmlEncode(from);

            Log.Info($"{from}");
            _xmppConnection.Send($"<presence to='{to_XmlEncoded}' from='{from_XmlEncoded}' />");

            await Task.CompletedTask;
        }

        async Task SendInventoryItemPresenceAvailable(string itemId, string from, string to)
        {
            Log.Info($"{from}");
            await SendItemPresenceAvailableCore(itemId, from, to, forXmppMucWithFirebatSupport: false);
        }

        async Task SendRoomItemPresenceAvailable(string roomId, string itemId)
        {
            var to = roomId + "/" + itemId;
            var from = $"{itemId}@{_componentDomain}";

            Log.Info($"Rez {from}");
            await SendItemPresenceAvailableCore(itemId, from, to);
        }

        async Task SendItemPresenceAvailableCore(string itemId, string from, string to, bool forXmppMucWithFirebatSupport = true)
        {
            if (_xmppConnection == null) { return; }

            var props = await MakeItemStub(itemId).GetProperties(PidSet.Public);

            var name = props.GetString(Pid.Name);
            if (string.IsNullOrEmpty(name)) { name = props.Get(Pid.Label); }
            if (string.IsNullOrEmpty(name)) { name = $"Item-{itemId}"; }

            var x = props.GetInt(Pid.RezzedX);

            var animationsUrl = props.GetString(Pid.AnimationsUrl);
            var imageUrl = props.GetString(Pid.ImageUrl);
            //if (!string.IsNullOrEmpty(animationsUrl)) {
            //    if (props.ContainsKey(Pid.ImageUrl)) {
            //        props.Delete(Pid.ImageUrl);
            //    }
            //}

            var identityJid = $"{itemId}@{_componentDomain}";
            var identityDigest = Math.Abs(string.GetHashCode(name + animationsUrl, StringComparison.InvariantCulture)).ToString(CultureInfo.InvariantCulture);

            var name_UrlEncoded = WebUtility.UrlEncode(name);
            var animationsUrl_UrlEncoded = string.IsNullOrEmpty(animationsUrl) ? "" : WebUtility.UrlEncode(animationsUrl);
            var imageUrl_UrlEncoded = string.IsNullOrEmpty(imageUrl) ? "" : WebUtility.UrlEncode(imageUrl);
            var digest_UrlEncoded = WebUtility.UrlEncode(identityDigest);
            var identitySrc = $"https://avatar.weblin.sui.li/identity/?imageUrl={imageUrl_UrlEncoded}?avatarUrl={animationsUrl_UrlEncoded}&nickname={name_UrlEncoded}&digest={digest_UrlEncoded}";

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

            _xmppConnection.Send(
#pragma warning disable format
                $"<presence to='{to_XmlEncoded}' from='{from_XmlEncoded}'>"
                    + $"<x xmlns='vp:props' type='item' provider='nine3q' {props_XmlEncoded_All} />"
                    + (forXmppMucWithFirebatSupport ? 
                          $"<x xmlns='firebat:user:identity' jid='{identityJid_XmlEncoded}' src='{identitySrc_XmlEncoded}' digest='{identityDigest_XmlEncoded}' />"
                        + $"<x xmlns='firebat:avatar:state'>{position_Node}</x>"
                        + $"<x xmlns='http://jabber.org/protocol/muc'><history seconds='0' maxchars='0' maxstanzas='0' /></x>"
                        : "")
                + $"</presence>"
#pragma warning restore format
            );

            await Task.CompletedTask;
        }

        async Task SendSubscriberPresenceUnavailableConfirmation(string from, string to)
        {
            Log.Info($"{from}");
            await SendPresenceUnvailableCore(from, to);
        }

        async Task SendInventoryItemPresenceUnavailable(string from, string to)
        {
            Log.Info($"{from}");
            await SendPresenceUnvailableCore(from, to);
        }

        async Task SendRoomItemPresenceUnvailable(string roomId, string itemId)
        {
            var roomResource = itemId;

            var to = $"{roomId}/{roomResource}";
            var from = $"{itemId}@{_componentDomain}";

            Log.Info($"Derez '{from}");
            await SendPresenceUnvailableCore(from, to);
        }

        async Task SendPresenceUnvailableCore(string from, string to)
        {
            var to_XmlEncoded = WebUtility.HtmlEncode(to);
            var from_XmlEncoded = WebUtility.HtmlEncode(from);

            _xmppConnection?.Send($"<presence to='{to_XmlEncoded}' from='{from_XmlEncoded}' type='unavailable' />");

            await Task.CompletedTask;
        }

        #endregion

    }

}