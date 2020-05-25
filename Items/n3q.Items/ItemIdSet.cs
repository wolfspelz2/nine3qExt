using System;
using System.Collections.Generic;
using n3q.Tools;

namespace n3q.Items
{
    public class ItemIdSet : HashSet<string>
    {
        const string JoinSeparator = " ";
        static readonly char[] SplitSeparator = new[] { JoinSeparator[0] };

        public static ItemIdSet FromString(string blankSeparatedListOfString)
        {
            var result = new ItemIdSet();
            if (Has.Value(blankSeparatedListOfString)) {
                var parts = blankSeparatedListOfString.Split(SplitSeparator, StringSplitOptions.RemoveEmptyEntries);
                foreach (string part in parts) {
                    result.Add(part);
                }
            }
            return result;
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
