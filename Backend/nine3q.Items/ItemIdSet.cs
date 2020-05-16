using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace nine3q.Items
{
    [Serializable]
    public class ItemIdSet : HashSet<ItemId>
    {
        const string JoinSeparator = " ";
        static char[] SplitSeparator = new[] { JoinSeparator[0] };

        public ItemIdSet() { }

        public ItemIdSet(string blankSeparatedListOfLong)
        {
            FromString(blankSeparatedListOfLong);
        }

        #region For [Serializable]

        protected ItemIdSet(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override void GetObjectData(SerializationInfo info, StreamingContext context) { base.GetObjectData(info, context); }

        #endregion

        public void FromString(string listOfLong)
        {
            var parts = listOfLong.Split(SplitSeparator, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts) {
                long value;
                if (long.TryParse(part, out value)) {
                    Add(new ItemId(value));
                }
            }
        }

        public ItemIdSet Clone()
        {
            var clone = new ItemIdSet();
            foreach (ItemId id in this) {
                clone.Add(id);
            }
            return clone;
        }

        public override string ToString()
        {
            return string.Join(JoinSeparator, this);
        }

    }
}
