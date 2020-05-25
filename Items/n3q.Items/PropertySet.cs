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
    }
}
