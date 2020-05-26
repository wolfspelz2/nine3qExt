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
        public readonly Dictionary<Pid, string> Dict;

        public PropertySet()
        {
            Dict = new Dictionary<Pid, string>();
        }

        public PropertySet(Dictionary<Pid, string> properties)
        {
            Dict = properties;
        }

        public PropertyValue this[Pid pid]
        {
            get { return new PropertyValue(Dict[pid]); }
            set { Dict[pid] = value.ToString(); }
        }

        public PropertyValue Get(Pid pid)
        {
            if (Dict.ContainsKey(pid)) {
                return new PropertyValue(Dict[pid]);
            }
            return new PropertyValue();
        }

        public void Set(Pid pid, string value)
        {
            Dict[pid] = value;
        }

        public void Set(Pid pid, bool value)
        {
            Dict[pid] = value.ToString();
        }

        public void Set(Pid pid, ItemIdSet ids)
        {
            Dict[pid] = ids.ToString();
        }
    }
}
