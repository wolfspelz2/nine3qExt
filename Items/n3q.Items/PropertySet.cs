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

        public PropertyValue Get(Pid pid)
        {
            if (ContainsKey(pid)) {
                return this[pid];
            }
            return new PropertyValue();
        }

        public void Set(Pid pid, string value)
        {
            this[pid] = new PropertyValue(value);
        }

        public void Set(Pid pid, long value)
        {
            this[pid] = new PropertyValue(value);
        }

        public void Set(Pid pid, double value)
        {
            this[pid] = new PropertyValue(value);
        }

        public void Set(Pid pid, bool value)
        {
            this[pid] = new PropertyValue(value);
        }

        public void Set(Pid pid, ItemIdSet ids)
        {
            this[pid] = new PropertyValue(ids);
        }
    }
}
