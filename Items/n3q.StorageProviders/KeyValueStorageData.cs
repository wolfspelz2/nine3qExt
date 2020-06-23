using System;
using System.Linq;
using System.Collections.Generic;

namespace n3q.StorageProviders
{
    [Serializable]
    public class KeyValueStorageData : Dictionary<string, object>
    {
        public KeyValueStorageData()
        {
        }

        protected KeyValueStorageData(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
        }
    }

    public static class KeyValueStorageDataExtensions
    {
        public static string Get(this KeyValueStorageData self, string key, string defaultValue)
        {
            if (self.ContainsKey(key)) {
                var value = self[key];
                return value.ToString();
            }
            return defaultValue;
        }

        public static long Get(this KeyValueStorageData self, string key, long defaultValue)
        {
            if (self.ContainsKey(key)) {
                var value = self[key];
                if (value is string stringValue) {
                    if (long.TryParse(stringValue, out var longValue)) {
                        return longValue;
                    }
                } else if (value is int intValue) {
                    return intValue;
                } else if (value is long longValue) {
                    return longValue;
                }
            }
            return defaultValue;
        }

        public static string ToSrpc(this KeyValueStorageData self)
        {
            return string.Join("\n", self.Select(kv => $"{kv.Key}: {kv.Value}"));
        }

        public static void FromSrpc(this KeyValueStorageData self, string srpc)
        {
            var lines = srpc.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines) {
                var parts = line.Split(new char[] { '=', ':', ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2) {
                    self.Add(parts[0], parts[1]);
                }
            }
        }
    }
}
