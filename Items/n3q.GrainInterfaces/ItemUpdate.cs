using System;
using n3q.Items;

namespace n3q.GrainInterfaces
{
    [Serializable]
    public class ItemUpdate
    {
        public enum Mode
        {
            Added,
            Changed,
            Removed,
        }

        public Mode What { get; set; }
        public string InventoryId { get; set; }
        public long Id { get; set; }
        public PidList Pids { get; set; }
        public ItemIdSet Parents { get; set; }

        public ItemUpdate(string inventoryId, long id, PidList pids, ItemIdSet parents, Mode mode)
        {
            What = mode;
            InventoryId = inventoryId;
            Id = id;
            Pids = pids;
            Parents = parents;
        }
    }
}
