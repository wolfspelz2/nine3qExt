using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using nine3q.Items.Aspects;
using nine3q.Items.Exceptions;
using nine3q.Tools;

namespace nine3q.Items
{
    public interface IInventoryChanges
    {
        bool IsChanged();
    }

    public class Inventory
    {
        #region Basic

        public string Name { get; set; }
        public bool IsActive { get; set; } = false;

        public Dictionary<long, Item> Items { get; } = new Dictionary<long, Item>();

        InventoryTransaction _currentTransaction = null;

        List<ItemChange> _changes = null;
        public List<ItemChange> Changes
        {
            get { if (_changes == null) { _changes = new List<ItemChange>(); } return _changes; }
            set { _changes = value; }
        }

        public Inventory Templates { get; set; }

        public ITimerManager Timers { get; set; } = new DummyTimerManager();

        readonly Dictionary<string, long> _names = new Dictionary<string, long>();

        const int MaxItemsPerInventory = 1000;

        public Inventory(string name = "")
        {
            Name = name;
        }

        public void Activate()
        {
            IsActive = true;
            foreach (var id in Items.Keys.ToList()) {
                Item(id).Activate();
            }
        }

        public bool IsItem(long id)
        {
            return Items.ContainsKey(id);
        }

        public bool IsItem(string name)
        {
            return _names.ContainsKey(name);
        }

        public Item Item(long id)
        {
            if (!Items.TryGetValue(id, out Item item)) {
                throw new ItemException(Name, id, "No such item");
            }
            return item;
        }

        public Item Item(string name)
        {
            if (!_names.TryGetValue(name, out long id)) {
                throw new ItemException(Name, ItemId.NoItem, $"No such item: name={name}");
            }
            if (!Items.TryGetValue(id, out Item item)) {
                throw new ItemException(Name, id, $"No such item: name={name}");
            }
            return item;
        }

        internal void SetName(string name, long id)
        {
            _names[name] = id;
        }

        internal void UnsetName(string name, long id)
        {
            if (_names.ContainsKey(name)) {
                _names.Remove(name);
            }
            Don.t = () => { var x = id; };
        }

        #endregion

        #region Interface

        public void CheckConflictingProperties(PropertySet properties, long forId)
        {
            if (properties == null) { return; }

            {
                var name = properties.GetString(Pid.Name);
                if (!string.IsNullOrEmpty(name)) {
                    var id = GetItemByName(name);
                    if (id != ItemId.NoItem) {
                        if (id == forId) {
                            // ok: just replacing name of an item with same name
                        } else {
                            throw new WrongItemPropertyException(Name, id, Pid.Name, $"Name={name} already exists");
                        }
                    }
                }
            }

            {
                var id = properties.GetItem(Pid.Id);
                if (id != ItemId.NoItem) {
                    if (IsItem(id)) {
                        throw new WrongItemPropertyException(Name, id, Pid.Id, $"Id={id} already exists");
                    }
                }
            }
        }

        long _currentItemId = 0;
        public long GetLastItemId() => _currentItemId;
        public void SetLastItemId(long itemId) { _currentItemId = itemId; }
        long GetNextItemId_NeverReUse()
        {
            if (_currentItemId == ItemId.NoItem && Items.Count > 0) {
                _currentItemId = Items.Keys.Max();
            }
            _currentItemId++;
            return _currentItemId;
        }

        long GetNextItemId()
        {
            return GetNextItemId_NeverReUse();
        }

        public Item CreateItem(PropertySet properties)
        {
            Contract.Requires(properties != null);
            CheckConflictingProperties(properties, ItemId.NoItem);

            if (Items.Keys.Count >= MaxItemsPerInventory) {
                throw new ItemException(Name, ItemId.NoItem, $"Exceeded max items per inventory: {MaxItemsPerInventory}");
            }

            var id = properties.GetItem(Pid.Id);
            if (id == ItemId.NoItem) {
                id = GetNextItemId();
            }
            if (id == ItemId.NoItem) {
                throw new ItemException(Name, id, "No item ID available");
            }

            var item = new Item(this, id, properties);
            Items.Add(id, item);

            var name = item.GetString(Pid.Name);
            if (!string.IsNullOrEmpty(name)) {
                SetName(name, id);
            }

            item.OnCreate();

            return item;
        }

        public bool DeleteItem(long id)
        {
            if (IsItem(id)) {
                var item = Item(id);
                item.OnDelete();

                var name = item.GetString(Pid.Name);
                if (!string.IsNullOrEmpty(name)) {
                    UnsetName(name, id);
                }

                Items.Remove(id);
                return true;
            }
            return false;
        }

        public void SetItemProperties(long id, PropertySet properties)
        {
            Contract.Requires(properties != null);
            properties.Delete(Pid.Id);
            CheckConflictingProperties(properties, id);

            var item = Item(id);
            foreach (var pair in properties) {
                item.Set(pair.Key, pair.Value);
            }
        }

        public PropertySet GetItemProperties(long id, PidList pids, bool native = false)
        {
            return Item(id).GetProperties(pids, native);
        }

        public int DeleteItemProperties(long id, PidList pids)
        {
            Contract.Requires(pids != null);
            pids.Remove(Pid.Id);
            var count = 0;

            var item = Item(id);
            foreach (var pid in pids) {
                var deleted = item.Delete(pid);
                if (deleted) {
                    count++;
                }
            }

            return count;
        }

        public long GetItemByName(string path)
        {
            Contract.Requires(path != null);
            if (_names.ContainsKey(path)) {
                return _names[path];
            }

            var pathSegments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var idList = Items.Keys.ToList();
            while (pathSegments.Count > 0) {
                var segment = pathSegments[0];
                pathSegments.RemoveAt(0);

                var pid = Pid.Name;
                var value = segment;
                var parts = segment.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1) {
                    pid = parts[0].ToEnum(Pid.NoProperty);
                    value = parts[1];
                }

                foreach (var id in idList) {
                    var item = Item(id);
                    if (Property.ToString(pid, item.Get(pid)) == value) {
                        if (pathSegments.Count == 0) {
                            return id;
                        } else {
                            if (item.IsContainer()) {
                                idList = item.GetItemSet(Pid.Contains).ToList();
                                break;
                            } else {
                                // Not found: intermediate part must be container
                                return ItemId.NoItem;
                            }
                        }
                    }
                }

            } // segments

            return ItemId.NoItem;
        }

        public ItemIdSet GetItemIds()
        {
            var ids = new ItemIdSet();
            foreach (var id in Items.Keys) {
                ids.Add(id);
            }
            return ids;
        }

        public ItemIdPropertiesCollection GetItemIdsAndValuesByProperty(Pid filterPid, PidList desiredProperties)
        {
            var idValueList = new ItemIdPropertiesCollection();

            foreach (var pair in Items) {
                var id = pair.Key;
                var item = pair.Value;
                var props = item.GetProperties(new PidList { filterPid });
                if (props.ContainsKey(filterPid)) {
                    var desiredProps = item.GetProperties(desiredProperties);
                    idValueList.Add(id, desiredProps);
                }
            }

            return idValueList;
        }

        public ItemIdPropertiesCollection GetItemIdsAndValuesByPropertyValue(PropertySet filterProperties, PidList desiredProperties)
        {
            Contract.Requires(filterProperties != null);
            var idValueList = new ItemIdPropertiesCollection();
            var filterPids = new PidList();

            foreach (var pair in filterProperties) {
                filterPids.Add(pair.Key);
            }

            foreach (var pair in Items) {
                var id = pair.Key;
                var item = pair.Value;
                var props = item.GetProperties(filterPids);

                var match = true;
                foreach (var filterPair in filterProperties) {
                    match = false;
                    if (props.ContainsKey(filterPair.Key)) {
                        if (Property.AreEquivalent(filterPair.Key, props[filterPair.Key], filterPair.Value)) {
                            match = true;
                        }
                    }
                    if (!match) {
                        break;
                    }
                }

                if (match) {
                    var desiredProps = item.GetProperties(desiredProperties);
                    idValueList.Add(id, desiredProps);
                }
            }

            return idValueList;
        }

        public int ExecuteItemAction(long id, string action, PropertySet arguments)
        {
            return Item(id).ExecuteAction(action, arguments);
        }

        #endregion

        #region Transaction

        public delegate void TransactionWrappedCode();

        public void Transaction(TransactionWrappedCode code)
        {
            Contract.Requires(code != null);
            using var t = BeginTransaction();
            try {
                code();
            } catch (Exception ex) {
                t.Cancel();
                Don.t = () => { var m = ex.Message; };
                throw;
            }
        }

        public InventoryTransaction BeginTransaction()
        {
            var t = new InventoryTransaction(this);

            if (_currentTransaction == null) {
                _currentTransaction = t;
                Changes = null;
            }

            return t;
        }

        public void CommitTransaction(InventoryTransaction t)
        {
            if (t != null && t == _currentTransaction) {
                _currentTransaction = null;
                Changes = t.GetChanges();
                t.ResetChanges();
            }
        }

        public void CancelTransaction(InventoryTransaction t)
        {
            if (t != null && t == _currentTransaction) {
                _currentTransaction = null;

                // Undo changes in reverse order
                var lifo = new Stack<ItemChange>();

                foreach (var change in t.GetChanges()) {
                    lifo.Push(change);
                }

                while (lifo.Count > 0) {
                    var change = lifo.Pop();
                    UndoItemChange(change);
                }

                t.ResetChanges();
            }
        }

        private void UndoItemChange(ItemChange change)
        {
            switch (change.What) {

                case ItemChange.Variant.CreateItem:
                    Items.Remove(change.ItemId);
                    break;

                case ItemChange.Variant.DeleteItem:
                    Items.Add(change.ItemId, change.Item);
                    break;

                case ItemChange.Variant.AddProperty:
                    if (IsItem(change.ItemId)) {
                        Items[change.ItemId].Delete(change.Pid);
                    }
                    break;

                case ItemChange.Variant.SetProperty:
                case ItemChange.Variant.DeleteProperty:
                    if (IsItem(change.ItemId)) {
                        Items[change.ItemId].Set(change.Pid, change.PreviousValue);
                    }
                    break;

                case ItemChange.Variant.AddItemToCollection:
                    if (IsItem(change.ItemId)) {
                        Items[change.ItemId].RemoveFromItemSet(change.Pid, change.ChildId);
                    }
                    break;

                case ItemChange.Variant.RemoveItemFromCollection:
                    if (IsItem(change.ItemId)) {
                        Items[change.ItemId].AddToItemSet(change.Pid, change.ChildId);
                    }
                    break;
            }
        }

        #endregion

        #region Events

        internal void OnCreateItem(ItemChange change)
        {
            if (_currentTransaction != null) {
                _currentTransaction.AddChange(change);
            }
        }

        internal void OnDeleteItem(ItemChange change)
        {
            if (_currentTransaction != null) {
                _currentTransaction.AddChange(change);
            }
        }

        internal void OnPropertyChange(ItemChange change)
        {
            if (_currentTransaction != null) {
                _currentTransaction.AddChange(change);
            }
        }

        #endregion

        #region Other

        public void AddChild(long containerId, long id, long slot)
        {
            Item(containerId).AsContainer().AddChild(Item(id), slot);
        }

        public void RemoveChild(long containerId, long id)
        {
            Item(containerId).AsContainer().RemoveChild(Item(id));
        }

        public ItemIdSet GetParentContainers(long id)
        {
            var parents = new ItemIdSet();

            var firstId = id;
            var lastId = ItemId.NoItem;
            id = Item(id).GetItem(Pid.Container);
            while (id != ItemId.NoItem) {
                if (id == lastId || id == firstId) {
                    // A bug in the data, but would loop infinitely
                    break;
                }
                lastId = id;
                parents.Add(id);
                id = Item(id).GetItem(Pid.Container);
            }

            return parents;
        }

        public ItemIdSet CollectChildren(long id, ItemIdSet list)
        {
            Contract.Requires(list != null);
            list.Add(id);
            var children = Item(id).GetItemSet(Pid.Contains);
            foreach (var childId in children) {
                list = CollectChildren(childId, list);
            }
            return list;
        }

        #endregion

        #region Transfer

        public ItemIdPropertiesCollection BeginItemTransfer(long id)
        {
            if (IsItem(id)) {
                var item = Item(id);
                item.SetString(Pid.TransferState, PropertyValue.TransferState.Source.ToString());

                var slot = ContainerAspect.NoSlot;
                var containerId = item.GetItem(Pid.Container);
                if (containerId != ItemId.NoItem) {
                    slot = item.GetInt(Pid.Slot);
                    RemoveChild(containerId, id);
                }

                var idProps = GetItemAndChildrenProperties(id, native: true);
                idProps[id][Pid.TransferState] = PropertyValue.TransferState.Destination;

                if (containerId != ItemId.NoItem) {
                    item.SetItem(Pid.TransferContainer, containerId);
                    item.SetInt(Pid.TransferSlot, slot);
                }
                return idProps;
            }
            return new ItemIdPropertiesCollection();
        }

        public ItemIdMap ReceiveItemTransfer(long id, long containerId, long slot, ItemIdPropertiesCollection idProps, PropertySet finallySetProperties, PidList finallyDeleteProperties)
        {
            var mapping = SetItemAndChildrenProperties(idProps);

            var newId = mapping[id];
            var item = Item(newId);
            if (containerId != ItemId.NoItem) {
                var container = Item(containerId);
                container.AsContainer().AddChild(item, slot);
            }

            SetItemProperties(newId, finallySetProperties);
            DeleteItemProperties(newId, finallyDeleteProperties);

            return mapping;
        }

        public void EndItemTransfer(long id)
        {
            if (IsItem(id)) {
                var item = Item(id);
                var transferState = item.GetEnum(Pid.TransferState, PropertyValue.TransferState.Unknown);
                if (transferState == PropertyValue.TransferState.Source) {
                    DeleteItem(id);
                }
                if (transferState == PropertyValue.TransferState.Destination) {
                    item.Delete(Pid.TransferContainer);
                    item.Delete(Pid.TransferSlot);
                    item.Delete(Pid.TransferState);
                    item.Activate();
                }
            }
        }

        public void CancelItemTransfer(long id)
        {
            if (IsItem(id)) {
                var item = Item(id);
                var transferState = item.GetEnum(Pid.TransferState, PropertyValue.TransferState.Unknown);
                if (transferState == PropertyValue.TransferState.Source) {
                    var containerId = item.GetItem(Pid.TransferContainer);
                    if (containerId != ItemId.NoItem) {
                        AddChild(containerId, id, item.GetInt(Pid.TransferSlot));
                    }

                    item.Delete(Pid.TransferContainer);
                    item.Delete(Pid.TransferSlot);
                    item.Delete(Pid.TransferState);
                }
                if (transferState == PropertyValue.TransferState.Destination) {
                    DeleteItem(id);
                }
            }
        }

        public ItemIdPropertiesCollection GetItemAndChildrenProperties(long id, bool native = false)
        {
            var idList = new ItemIdSet();
            idList = CollectChildren(id, idList);

            var idProps = new ItemIdPropertiesCollection();
            foreach (var itemId in idList) {
                idProps[itemId] = GetItemProperties(itemId, PidList.All, native);
            }
            return idProps;
        }

        public ItemIdMap SetItemAndChildrenProperties(ItemIdPropertiesCollection idProps)
        {
            Contract.Requires(idProps != null);
            var old2new = new ItemIdMap();

            foreach (var pair in idProps) {
                old2new[pair.Key] = GetNextItemId();
            }

            foreach (var idPropPair in idProps) {
                var oldId = idPropPair.Key;
                var oldProps = idPropPair.Value;
                var newProps = new PropertySet();
                foreach (var oldPropPair in oldProps) {
                    var pid = oldPropPair.Key;
                    var value = oldPropPair.Value;

                    switch (Property.Get(pid).Type) {
                        case Property.Type.Item:
                            if (!(value is long)) {
                                value = value.ToString();
                            }
                            var oldItemId = (long)value;
                            if (old2new.ContainsKey(oldItemId)) {
                                value = old2new[oldItemId];
                            }
                            break;
                        case Property.Type.ItemSet:
                            if (!(value is ItemIdSet)) {
                                value = new ItemIdSet(value.ToString());
                            }
                            var newItemIdSet = new ItemIdSet();
                            foreach (var idSetElem in value as ItemIdSet) {
                                if (old2new.ContainsKey(idSetElem)) {
                                    newItemIdSet.Add(old2new[idSetElem]);
                                } else {
                                    newItemIdSet.Add(idSetElem);
                                }
                            }
                            value = newItemIdSet;
                            break;
                        default:
                            break;
                    }

                    newProps[oldPropPair.Key] = value;
                }

                newProps[Pid.Id] = old2new[oldId];
                CreateItem(newProps);
            }

            return old2new;
        }

        #endregion

        #region Timer

        internal void StartTimer(long id, string timer, TimeSpan interval)
        {
            Timers.StartTimer(id, timer, interval);
        }

        internal void CancelTimer(long id, string timer)
        {
            Timers.CancelTimer(id, timer);
        }

        public void OnTimer(long id, string timer)
        {
            Item(id).OnTimer(timer);
        }

        #endregion
    }
}
