using System;
using System.Collections.Generic;
using n3q.Items;

namespace n3q.Items
{
    [Serializable]
    public class ItemChange
    {
        public enum Mode
        {
            SetProperty,
            AddToList,
            RemoveFromList,
            DeleteProperty,
            DeleteItem,
        }

        public Mode What { get; }
        public Pid Pid { get; }

        // PropertyChanged: Value contains the new property value
        // AddedToItemList: Value contains the added item id
        // RemovedFromItemList: Value contains the removed item id
        public PropertyValue Value { get; set; }

        public ItemChange(Mode what, Pid pid, PropertyValue value)
        {
            What = what;
            Pid = pid;
            Value = value;
        }
    }

    [Serializable]
    public class ItemUpdate
    {
        public string ItemId { get; }
        public List<ItemChange> Changes = new List<ItemChange>();

        public ItemUpdate(string itemId, List<ItemChange> changes)
        {
            ItemId = itemId;
            Changes = changes;
        }
    }

}
