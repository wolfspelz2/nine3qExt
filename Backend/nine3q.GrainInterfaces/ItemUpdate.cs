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

        public long Id { get; set; }
        public ItemIdSet Parents { get; set; }
        public Mode What { get; set; }

        public ItemUpdate(long id, ItemIdSet parents, Mode mode)
        {
            Id = id;
            Parents = parents;
            What = mode;
        }
    }
}
