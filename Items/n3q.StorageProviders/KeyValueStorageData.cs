using System;
using System.Linq;
using System.Collections.Generic;

namespace n3q.StorageProviders
{
    [Serializable]
    public class KeyValueStorageData : Dictionary<string, object>
    {
        internal string ToSrpc()
        {
            return string.Join("\n", this.Select(kv => $"{kv.Key}: {kv.Value}"));
        }

        internal void FromSrpc(string srpc)
        {
            var lines = srpc.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines) {
                var parts = line.Split(new char[] { '=', ':', ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2) {
                    Add(parts[0], parts[1]);
                }
            }
        }
    }
}
