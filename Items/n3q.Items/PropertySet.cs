using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using n3q.Tools;

namespace n3q.Items
{
    public class PropertySet
    {
        private readonly Dictionary<Pid, string> _properties;

        public PropertySet(Dictionary<Pid, string> properties)
        {
            _properties = properties;
        }

        public PropertyValue this[Pid pid]
        {
            get { return new PropertyValue(_properties[pid]); }
            set { _properties[pid] = value.ToString(); }
        }

        public PropertyValue Get(Pid pid)
        {
            if (_properties.ContainsKey(pid)) {
                return new PropertyValue(_properties[pid]);
            }
            return new PropertyValue();
        }

        public void Set(Pid pid, string value)
        {
            _properties[pid] = value;
        }

        public void Set(Pid pid, bool value)
        {
            _properties[pid] = value.ToString();
        }

        public void Set(Pid pid, ItemIdSet ids)
        {
            _properties[pid] = ids.ToString();
        }
    }
}
