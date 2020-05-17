using System;
using nine3q.Items;

namespace nine3q.GrainInterfaces
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

        public ItemId Id { get; set; }
        public ItemIdList Parents { get; set; }
        public Mode What { get; set; }

        public ItemUpdate(ItemId id, ItemIdList parents, Mode mode)
        {
            Id = id;
            Parents = parents;
            What = mode;
        }
    }
}
