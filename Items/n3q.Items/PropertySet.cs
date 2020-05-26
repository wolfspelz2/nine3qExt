using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using n3q.Tools;

namespace n3q.Items
{
    public class PropertySet : Dictionary<Pid, PropertyValue>
    {
        public PropertySet()
        {
        }

        public PropertySet(Dictionary<Pid, string> properties)
        {
            if (properties != null) {
                _ = properties.Select(pair => this[pair.Key] = new PropertyValue(pair.Value));
            }
        }

        public void Set(Pid pid, string value) { this[pid] = new PropertyValue(value); }
        public void Set(Pid pid, long value) { this[pid] = new PropertyValue(value); }
        public void Set(Pid pid, double value) { this[pid] = new PropertyValue(value); }
        public void Set(Pid pid, bool value) { this[pid] = new PropertyValue(value); }
        public void Set(Pid pid, ItemIdSet ids) { this[pid] = new PropertyValue(ids); }

        public PropertyValue Get(Pid pid)
        {
            if (ContainsKey(pid)) {
                return this[pid];
            }
            return new PropertyValue();
        }

        public string GetString(Pid pid) { return (string)Get(pid); }
        public long GetInt(Pid pid) { return (long)Get(pid); }
        public double GetFloat(Pid pid) { return (double)Get(pid); }
        public bool GetBool(Pid pid) { return (bool)Get(pid); }
        public ItemIdSet GetItemIdSet(Pid pid) { return (ItemIdSet)Get(pid); }

        public void Delete(Pid pid)
        {
            if (ContainsKey(pid)) {
                Remove(pid);
            }
        }

    }
}
