using System.Collections.Generic;
using System.Linq;

namespace n3q.Tools
{
    public static class IEnumerableExtensions
    {
        public static Dictionary<string, string> ToStringDictionary(this IEnumerable<KeyValuePair<string, string>> self)
        {
            return self.ToDictionary(kv => kv.Key, kv => kv.Value);
        }

    }
}
