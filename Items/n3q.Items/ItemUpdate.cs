using System;
using n3q.Items;

namespace n3q.Items
{
    [Serializable]
    public class ItemUpdate
    {
        public enum Mode
        {
            PropertyChanged,
            AddedToItemList,
            RemovedFromItemList,
        }

        public Mode What { get; set; }
        public string ItemId { get; set; }
        public Pid Pid { get; set; }

        // PropertyChanged: Value contains the new property value
        // AddedToItemList: Value contains the added item id
        // RemovedFromItemList: Value contains the removed item id
        public PropertyValue Value { get; set; }

        public ItemUpdate(Mode what, string itemId, Pid pid, PropertyValue value)
        {
            What = what;
            ItemId = itemId;
            Pid = pid;
            Value = value;
        }
    }
}
