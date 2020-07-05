using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace JsonPath
{
    public static class Extensions
    {
        public static JsonPath.Dictionary ToDictionary(this IEnumerable<KeyValuePair<string, Node>> self)
        {
            var dict = new Dictionary();
            foreach (var pair in self) {
                dict.Add(pair.Key, pair.Value);
            }
            return dict;
        }
    }
}
