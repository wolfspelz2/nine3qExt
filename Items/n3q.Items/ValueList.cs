using System;
using System.Collections.Generic;
using n3q.Tools;

namespace n3q.Items
{
    public class ValueList : List<string>
    {
        const string JoinSeparator = " ";
        static readonly char[] SplitSeparator = new[] { JoinSeparator[0] };

        public static ValueList FromString(string blankSeparatedListOfString)
        {
            var result = new ValueList();
            if (Has.Value(blankSeparatedListOfString)) {
                var parts = blankSeparatedListOfString.Split(SplitSeparator, StringSplitOptions.RemoveEmptyEntries);
                foreach (string part in parts) {
                    result.Add(part);
                }
            }
            return result;
        }

        public static ValueList From(IEnumerable<string> list)
        {
            var result = new ValueList();
            foreach (string listItem in list) {
                result.Add(listItem);
            }
            return result;
        }

        public override string ToString()
        {
            return string.Join(JoinSeparator, this);
        }
    }

}
