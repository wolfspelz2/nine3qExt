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
        // AddedToItemList: Value contains the added item id AND Length has the new list size
        // RemovedFromItemList: Value contains the removed item id AND Length has the new list size
        public PropertyValue Value { get; set; }
        public long Length { get; set; }

        public ItemChange(Mode what, Pid pid, PropertyValue value, long length)
        {
            What = what;
            Pid = pid;
            Value = value;
            Length = length;
        }
    }

    [Serializable]
    public class ItemUpdate
    {
        public string ItemId { get; }
        public string ParentId { get; }
        public List<ItemChange> Changes = new List<ItemChange>();

        public ItemUpdate(string itemId, string parentId, List<ItemChange> changes)
        {
            ItemId = itemId;
            ParentId = parentId;
            Changes = changes;
        }
    }
}
