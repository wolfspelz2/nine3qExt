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
                await SendPresenceAvailable(roomId, itemId);
                roomItem.State = RoomItem.RezState.Rezzing;
            }
        }

        async Task SendAllItemPresenceToInventorySubscriber(string inventoryItemId, string presenceFrom, string presenceTo)
        {
            var roomItemJid = new XmppJid(presenceFrom);

            var itemList = await MakeItemStub(inventoryItemId).GetItemIdList(Pid.Contains);
            foreach (var itemId in itemList) {

                var itemFrom = roomItemJid.Base + "/" + itemId;
                await SendInventoryItemPresenceAvailable(itemId, presenceFrom, presenceTo);

                await SendSubscriberPresenceConfirmation(presenceFrom, presenceTo);

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

        bool IsInventory(string inventoryItemId)
        {
            return GetInventory(inventoryItemId) != null;
        }

        Inventory GetInventory(string inventoryItemId)
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
                if (!inv.Subscribers.ContainsKey(clientJid)) {
                    inventorySubscriber = new InventorySubscriber(userId, clientJid);
                    inv.Subscribers.Add(clientJid, inventorySubscriber);
                }
            }

            return inventorySubscriber;
        }

        void RemoveInventorySubscriber(string userId, string clientJid)
        {
            lock (_mutex) {
                if (_inventories.TryGetValue(userId, out var inv)) {
                    if (inv.Subscribers.ContainsKey(clientJid)) {
                        inv.Subscribers.Remove(clientJid);
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

                    managedRoom.Items.Add(itemId, roomItem);
                    _roomItems.Add(itemId, roomItem);
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
                    case "itemAction": await Connection_OnItemAction(stanza); break;
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

        async Task Connection_OnPresenceAvailable(XmppPresence stanza)
        {
            var jid = new XmppJid(stanza.From);
            var roomId = jid.Base;
            var itemId = jid.Resource;

            var roomItem = GetRoomItem(roomId, itemId);
            if (roomItem != null) {
                if (roomItem.State != RoomItem.RezState.Rezzing) {
                    Log.Warning($"Unexpected presence-available: room={roomId} item={itemId}", nameof(Connection_OnPresenceAvailable));
                } else {
                    Log.Info($"Joined room {roomId} {itemId}");
                    roomItem.State = RoomItem.RezState.Rezzed;
                    await GetIWorker().AspectAction(itemId, Pid.RezableAspect, nameof(Rezable.OnRezzed));
                }
            } else {

                // Maybe a user
                var participantJid = stanza.To;
                var clientJid = stanza.From;
                var userToken = new XmppJid(participantJid).User;
                var inventoryItemId = await GetInventoryByUserToken(userToken);
                if (Has.Value(inventoryItemId)) {

                    if (!IsInventorySubscriber(userToken, stanza.From)) {
                        AddInventorySubscriber(userToken, inventoryItemId, stanza.To, stanza.From);
                        await SendAllItemPresenceToInventorySubscriber(inventoryItemId, stanza.To, stanza.From);
                    }

                }
            }

            await Task.CompletedTask;
        }

        async Task<string> GetInventoryByUserToken(string userToken)
        {
            await Task.CompletedTask;
            var inventoryItemId = userToken == "user1" ? "User1" : "";
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

                // Just in case, should already be removed after sending presence-unavailable
                await RemoveRoomItem(roomId, itemId);

            } else {

                // Maybe a user
                //var participantJid = stanza.To;
                var clientJid = stanza.From;
                var userToken = new XmppJid(stanza.To).User;
                if (!IsInventorySubscriber(userToken, clientJid)) {
                    RemoveInventorySubscriber(userToken, clientJid);
                }

            }

            await Task.CompletedTask;
        }

        async Task Connection_OnItemAction(XmppMessage message)
        {
            var userId = "";
            var itemId = "";
            var actionName = "";
            var args = new Dictionary<string, string>();
            foreach (var pair in message.Cmd) {
                switch (pair.Key) {
                    case "method": break;
                    case "xmlns": break;
                    case "user": userId = pair.Value; break;
                    case "item": itemId = pair.Value; break;
                    case "action": actionName = pair.Value; break;
                    default: args[pair.Key] = pair.Value; break;
                }
            }

            Log.Info($"ItemAction user={userId} item={itemId} action={actionName}");

            switch (actionName) {
                case nameof(Rezable.Action.Rez): {
                    if (Has.Value(userId) && Has.Value(itemId)) {
                        if (await MakeItemStub(itemId).GetBool(Pid.RezableAspect)) {
                            var roomId = message.Cmd.ContainsKey("to") ? message.Cmd["to"] : "";
                            if (Has.Value(roomId)) {
                                _ = await AddRoomItem(roomId, itemId);

                                var room = MakeItemStub(roomId);
                                if (!await room.Get(Pid.ContainerAspect)) {
                                    await room.WithTransaction(async self => { await self.Set(Pid.ContainerAspect, true); });
                                }

                                var x = message.Cmd.ContainsKey("x") ? message.Cmd["x"] : "";
                                if (!long.TryParse(x, NumberStyles.Any, CultureInfo.InvariantCulture, out long posX)) {
                                    posX = 200;
                                }
                                await GetIWorker().AspectAction(itemId, Pid.RezableAspect, nameof(Rezable.Action.Rez), new PropertySet { [Pid.RezableRezTo] = roomId, [Pid.RezableRezX] = posX });

                            }
                        }
                    }
                }
                break;

                case nameof(Rezable.Action.Derez): {
                    if (Has.Value(userId) && Has.Value(itemId)) {
                        if (await MakeItemStub(itemId).GetBool(Pid.RezableAspect)) {
                            var inventoryId = message.Cmd.ContainsKey("to") ? message.Cmd["to"] : "";
                            if (Has.Value(inventoryId)) {
                                var roomItem = GetRoomItem(itemId);
                                if (roomItem != null) {
                                    await GetIWorker().AspectAction(itemId, Pid.RezableAspect, nameof(Rezable.Action.Derez), new PropertySet { [Pid.RezableDerezTo] = inventoryId });
                                }

                            }
                        }
                    }
                }
                break;
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
            } else if (IsInventory(update.ItemId)) {
                await OnItemUpdateInventory(update);
            } else if (IsInventory(update.ParentId)) {
                await OnItemUpdateInventoryItem(update);
            }
        }

        private async Task OnItemUpdateInventoryItem(ItemUpdate update)
        {
            var itemId = update.ItemId;
            var inv = GetInventory(update.ParentId);
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
                    var inv = GetInventory(update.ItemId);
                    if (inv != null) {
                        var itemFrom = new XmppJid(inv.ParticipantJid).Base + "/" + itemId;
                        foreach (var pair in inv.Subscribers) {
                            var subscriber = pair.Value;
                            await SendInventoryItemPresenceAvailable(itemId, itemFrom, subscriber.ClientJid);
                        }
                    }
                } else if (change.Pid == Pid.Contains && change.What == ItemChange.Mode.RemoveFromList) {
                    var itemId = change.Value;
                    var inv = GetInventory(update.ItemId);
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
                        await SendPresenceAvailable(roomItem.RoomId, roomItem.ItemId);
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
                    await SendPresenceAvailable(roomId, itemId);
                    roomItem.State = RoomItem.RezState.Rezzing;

                } else if (change.Pid == Pid.Contains && change.What == ItemChange.Mode.RemoveFromList) {
                    var roomId = update.ItemId;
                    var itemId = change.Value;
                    var roomItem = GetRoomItem(roomId, itemId);
                    if (roomItem != null) {
                        await SendPresenceUnvailable(roomItem.RoomId, roomItem.ItemId);
                        roomItem.State = RoomItem.RezState.Derezzing;
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

        async Task SendSubscriberPresenceConfirmation(string presenceFrom, string presenceTo)
        {
            if (_xmppConnection == null) { return; }

            var to = presenceTo;
            var from = presenceFrom;

            var to_XmlEncoded = WebUtility.HtmlEncode(to);
            var from_XmlEncoded = WebUtility.HtmlEncode(from);

            _xmppConnection.Send(
#pragma warning disable format
                $"<presence to='{to_XmlEncoded}' from='{from_XmlEncoded}' />"
#pragma warning restore format
            );

            await Task.CompletedTask;
        }

        async Task SendInventoryItemPresenceAvailable(string itemId, string from, string to)
        {
            Log.Info($"Add to inventory {from}");
            await SendPresenceAvailableCore(itemId, from, to);
        }

        async Task SendPresenceAvailable(string roomId, string itemId)
        {
            var to = roomId + "/" + itemId;
            var from = $"{itemId}@{_componentDomain}";

            Log.Info($"Rez {from}");
            await SendPresenceAvailableCore(itemId, from, to);
        }

        async Task SendPresenceAvailableCore(string itemId, string from, string to)
        {
            if (_xmppConnection == null) { return; }

            var props = await MakeItemStub(itemId).GetProperties(PidSet.Public);

            var name = props.GetString(Pid.Name);
            if (string.IsNullOrEmpty(name)) { name = props.Get(Pid.Label); }
            if (string.IsNullOrEmpty(name)) { name = $"Item-{itemId}"; }

            var x = props.GetInt(Pid.RezzedX);

            var animationsUrl = props.GetString(Pid.AnimationsUrl);
            if (!string.IsNullOrEmpty(animationsUrl)) {
                if (props.ContainsKey(Pid.Image100Url)) {
                    props.Delete(Pid.Image100Url);
                }
            }

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

            _xmppConnection.Send(
#pragma warning disable format
                $"<presence to='{to_XmlEncoded}' from='{from_XmlEncoded}'>"
                    + $"<x xmlns='vp:props' type='item' service='nine3q' {props_XmlEncoded_All} />"
                    + $"<x xmlns='firebat:user:identity' jid='{identityJid_XmlEncoded}' src='{identitySrc_XmlEncoded}' digest='{identityDigest_XmlEncoded}' />"
                    + $"<x xmlns='firebat:avatar:state'>{position_Node}</x>"
                    + $"<x xmlns='http://jabber.org/protocol/muc'><history seconds='0' maxchars='0' maxstanzas='0' /></x>"
                + $"</presence>"
#pragma warning restore format
            );

            await Task.CompletedTask;
        }

        async Task SendInventoryItemPresenceUnavailable(string from, string to)
        {
            Log.Info($"Remove from inventory {from}");
            await SendPresenceUnvailableCore(from, to);
        }

        async Task SendPresenceUnvailable(string roomId, string itemId)
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