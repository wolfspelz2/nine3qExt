using System;
using System.Collections.Generic;

namespace n3q.Items
{
    public class ItemIdSet : HashSet<string>
    {
        const string JoinSeparator = " ";
        static readonly char[] SplitSeparator = new[] { JoinSeparator[0] };

        public void FromString(string listOfString)
        {
            if (listOfString == null) return;
            var parts = listOfString.Split(SplitSeparator, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts) {
                Add(part);
            }
        }

        public ItemIdSet Clone()
        {
            var clone = new ItemIdSet();
            foreach (var id in this) {
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
