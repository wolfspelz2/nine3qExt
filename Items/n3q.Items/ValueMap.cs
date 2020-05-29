using System;
using System.Collections.Generic;
using System.Linq;
using n3q.Tools;

namespace n3q.Items
{
    public class ValueMap : Dictionary<string, string>
    {
        const string JoinPairSeparator = " ";
        static readonly char[] SplitPairSeparator = new[] { JoinPairSeparator[0] };
        const string JoinPartSeparator = "=";
        static readonly char[] SplitPartSeparator = new[] { JoinPairSeparator[0] };

        public static ValueMap FromString(string blankSeparatedListOfEqualsSeparatedPairs)
        {
            var result = new ValueMap();
            if (Has.Value(blankSeparatedListOfEqualsSeparatedPairs)) {
                var pairs = blankSeparatedListOfEqualsSeparatedPairs.Split(SplitPairSeparator, StringSplitOptions.RemoveEmptyEntries);
                foreach (var pair in pairs) {
                    var parts = pair.Split(SplitPartSeparator, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2) {
                        result.Add(parts[0], parts[1]);
                    }
                }
            }
            return result;
        }

        public static ValueMap From(IEnumerable<KeyValuePair<string, string>> map)
        {
            var result = new ValueMap();
            foreach (var pair in map) {
                result.Add(pair.Key, pair.Value);
            }
            return result;
        }

        public override string ToString()
        {
            return string.Join(JoinPairSeparator, this.Select(pair => pair.Key + JoinPartSeparator + pair.Value));
        }
    }

}
