using System;
using System.Collections.Generic;
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

        public Dictionary<long, Item> Items { get; set; } = new Dictionary<long, Item>();

        InventoryTransaction CurrentTransaction = null;

        List<ItemChange> _changes = null;
        public List<ItemChange> Changes
        {
            get { if (_changes == null) { _changes = new List<ItemChange>(); } return _changes; }
            set { _changes = value; }
        }

        public Inventory Templates { get; set; }

        public ITimerManager Timers { get; set; } = new DummyTimerManager();

        readonly Dictionary<string, long> Names = new Dictionary<string, long>();
        
        readonly int MaxItemsPerInventory = 1000;

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
            return Names.ContainsKey(name);
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
            if (!Names.TryGetValue(name, out long id)) {
                throw new ItemException(Name, long.NoItem, $"No such item: name={name}");
            }
            if (!Items.TryGetValue(id, out Item item)) {
                throw new ItemException(Name, id, $"No such item: name={name}");
            }
            return item;
        }

        internal void SetName(string name, long id)
        {
            Names[name] = id;
        }

        internal void UnsetName(string name, long id)
        {
            if (Names.ContainsKey(name)) {
                Names.Remove(name);
            }
            Utils.Dont  = () => { var x = id; };
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
                    if (id != long.NoItem) {
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
                if (id != long.NoItem) {
                    if (IsItem(id)) {
                        throw new WrongItemPropertyException(Name, id, Pid.Id, $"Id={id} already exists");
                    }
                }
            }
        }

        long _currentlong = 0;
        long GetNextlong_NeverReUse()
        {
            if (_currentlong == 0 && Items.Count > 0) {
                _currentlong = (long)Items.Keys.Max();
            }
            _currentlong++;
            return new long(_currentlong);
        }

        long GetNextlong()
        {
            return GetNextlong_NeverReUse();
        }

        public Item CreateItem(PropertySet properties)
        {
            CheckConflictingProperties(properties, long.NoItem);

            if (Items.Keys.Count >= MaxItemsPerInventory) {
                throw new ItemException(Name, long.NoItem, $"Exceeded max items per inventory: {MaxItemsPerInventory}");
            }

            var id = properties.GetItem(Pid.Id);
            if (id == long.NoItem) {
                id = GetNextlong();
            }
            if (id == long.NoItem) {
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
            if (Names.ContainsKey(path)) {
                return Names[path];
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
                var type = Property.Get(pid).Type;

                foreach (var id in idList) {
                    var item = Item(id);
                    if (Property.ToString(type, item.Get(pid)) == value) {
                        if (pathSegments.Count == 0) {
                            return id;
                        } else {
                            if (item.IsContainer()) {
                                idList = item.GetItemSet(Pid.Contains).ToList();
                                break;
                            } else {
                                // Not found: intermediate part must be container
                                return long.NoItem;
                            }
                        }
                    }
                }

            } // segments

            return long.NoItem;
        }

        public longSet GetItems()
        {
            var ids = new longSet();
            foreach (var id in Items.Keys) {
                ids.Add(id);
            }
            return ids;
        }

        public longPropertiesCollection GetlongsAndValuesByProperty(Pid filterPid, PidList desiredProperties)
        {
            var idValueList = new longPropertiesCollection();

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

        public longPropertiesCollection GetlongsAndValuesByPropertyValue(PropertySet filterProperties, PidList desiredProperties)
        {
            var idValueList = new longPropertiesCollection();
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
                        var prop = Property.Get(filterPair.Key);
                        if (Property.AreEquivalent(prop.Type, props[filterPair.Key], filterPair.Value)) {
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
            using (var t = BeginTransaction()) {
                try {
                    code();
                } catch (Exception ex) {
                    t.Cancel();
                    Utils.Dont = () => { var m = ex.Message; };
                    throw;
                }
            }
        }

        public InventoryTransaction BeginTransaction()
        {
            var t = new InventoryTransaction(this);

            if (CurrentTransaction == null) {
                CurrentTransaction = t;
                Changes = null;
            }

            return t;
        }

        public void _CommitTransaction(InventoryTransaction t)
        {
            if (t == CurrentTransaction) {
                CurrentTransaction = null;
                Changes = t.GetChanges();
                t.ResetChanges();
            }
        }

        public void _CancelTransaction(InventoryTransaction t)
        {
            if (t == CurrentTransaction) {
                CurrentTransaction = null;

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
                    Items.Remove(change.long);
                    break;

                case ItemChange.Variant.DeleteItem:
                    Items.Add(change.long, change.Item);
                    break;

                case ItemChange.Variant.AddProperty:
                    if (IsItem(change.long)) {
                        Items[change.long].Delete(change.Pid);
                    }
                    break;

                case ItemChange.Variant.SetProperty:
                case ItemChange.Variant.DeleteProperty:
                    if (IsItem(change.long)) {
                        Items[change.long].Set(change.Pid, change.PreviousValue);
                    }
                    break;

                case ItemChange.Variant.AddItemToCollection:
                    if (IsItem(change.long)) {
                        Items[change.long].RemoveFromItemSet(change.Pid, change.ChildId);
                    }
                    break;

                case ItemChange.Variant.RemoveItemFromCollection:
                    if (IsItem(change.long)) {
                        Items[change.long].AddToItemSet(change.Pid, change.ChildId);
                    }
                    break;
            }
        }

        #endregion

        #region Events

        internal void OnCreateItem(ItemChange change)
        {
            if (CurrentTransaction != null) {
                CurrentTransaction.AddChange(change);
            }
        }

        internal void OnDeleteItem(ItemChange change)
        {
            if (CurrentTransaction != null) {
                CurrentTransaction.AddChange(change);
            }
        }

        internal void OnPropertyChange(ItemChange change)
        {
            if (CurrentTransaction != null) {
                CurrentTransaction.AddChange(change);
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

        public longList GetParentContainers(long id)
        {
            var parents = new longList();

            var firstId = id;
            var lastId = long.NoItem;
            id = Item(id).GetItem(Pid.Container);
            while (id != long.NoItem) {
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

        public longSet CollectChildren(long id, longSet list)
        {
            list.Add(id);
            var children = Item(id).GetItemSet(Pid.Contains);
            foreach (var childId in children) {
                list = CollectChildren(childId, list);
            }
            return list;
        }

        #endregion

        #region Transfer

        public longPropertiesCollection BeginItemTransfer(long id)
        {
            if (IsItem(id)) {
                var item = Item(id);
                item.SetString(Pid.TransferState, PropertyValue.TransferState.Source.ToString());

                var slot = ContainerAspect.NoSlot;
                var containerId = item.GetItem(Pid.Container);
                if (containerId != long.NoItem) {
                    slot = item.GetInt(Pid.Slot);
                    RemoveChild(containerId, id);
                }

                var idProps = GetItemAndChildrenProperties(id, native: true);
                idProps[id][Pid.TransferState] = PropertyValue.TransferState.Destination;

                if (containerId != long.NoItem) {
                    item.SetItem(Pid.TransferContainer, containerId);
                    item.SetInt(Pid.TransferSlot, slot);
                }
                return idProps;
            }
            return new longPropertiesCollection();
        }

        public longMap ReceiveItemTransfer(long id, long containerId, long slot, longPropertiesCollection idProps, PropertySet setProperties, PidList removeProperties)
        {
            var mapping = SetItemAndChildrenProperties(idProps);

            var newId = mapping[id];
            var item = Item(newId);
            if (containerId != long.NoItem) {
                var container = Item(containerId);
                container.AsContainer().AddChild(item, slot);
            }

            SetItemProperties(newId, setProperties);
            DeleteItemProperties(newId, removeProperties);

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
                    if (containerId != long.NoItem) {
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

        public longPropertiesCollection GetItemAndChildrenProperties(long id, bool native = false)
        {
            var idList = new longSet();
            idList = CollectChildren(id, idList);

            var idProps = new longPropertiesCollection();
            foreach (var long in idList) {
                idProps[long] = GetItemProperties(long, PidList.All, native);
            }
            return idProps;
        }

        public longMap SetItemAndChildrenProperties(longPropertiesCollection idProps)
        {
            var old2new = new longMap();

            foreach (var pair in idProps) {
                old2new[pair.Key] = GetNextlong();
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
                                value = new long(value.ToString());
                            }
                            var oldlong = value as long;
                            if (old2new.ContainsKey(oldlong)) {
                                value = old2new[oldlong];
                            }
                            break;
                        case Property.Type.ItemSet:
                            if (!(value is longSet)) {
                                value = new longSet(value.ToString());
                            }
                            var newlongSet = new longSet();
                            foreach (var idSetElem in value as longSet) {
                                if (old2new.ContainsKey(idSetElem)) {
                                    newlongSet.Add(old2new[idSetElem]);
                                } else {
                                    newlongSet.Add(idSetElem);
                                }
                            }
                            value = newlongSet;
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
