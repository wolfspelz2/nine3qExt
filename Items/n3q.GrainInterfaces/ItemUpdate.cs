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
        public string Id { get; set; }
        public PidSet Pids { get; set; }
        public ItemIdSet Parents { get; set; }

        public ItemUpdate(string id, PidSet pids, ItemIdSet parents, Mode mode)
        {
            What = mode;
            Id = id;
            Pids = pids;
            Parents = parents;
        }
    }
}
