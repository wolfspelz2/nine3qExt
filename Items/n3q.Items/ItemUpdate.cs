using System;
using System.Collections.Generic;
using n3q.Items;

namespace n3q.Items
{
    [Serializable]
    public class PropertyChange
    {
        public enum Mode
        {
            SetProperty,
            AddToSet,
            RemoveFromSet,
            DeleteProperty,
        }

        public Mode What { get; set; }
        public Pid Pid { get; set; }

        // PropertyChanged: Value contains the new property value
        // AddedToItemList: Value contains the added item id
        // RemovedFromItemList: Value contains the removed item id
        public PropertyValue Value { get; set; }

        public PropertyChange(Mode what, Pid pid, PropertyValue value)
        {
            What = what;
            Pid = pid;
            Value = value;
        }
    }

    [Serializable]
    public class ItemUpdate
    {
        public string ItemId { get; set; }
        public List<PropertyChange> Changes = new List<PropertyChange>();

        public ItemUpdate(string itemId, List<PropertyChange> changes)
        {
            ItemId = itemId;
            Changes = changes;
        }
    }

}
