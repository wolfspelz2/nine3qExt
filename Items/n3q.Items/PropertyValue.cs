using System;
using System.Globalization;
using System.Linq;
using n3q.Tools;

namespace n3q.Items
{
    public class PropertyValue
    {
        string _value;

        public PropertyValue()
        {
            _value = "";
        }

        public PropertyValue(string value)
        {
            _value = value;
        }

        public void Set(string value)
        {
            _value = value;
        }

        public static implicit operator string(PropertyValue pv) { return pv._value; }

        public static implicit operator bool(PropertyValue pv) { return pv._value.IsTrue(); }

        public static implicit operator long(PropertyValue pv)
        {
            _ = long.TryParse(pv._value, NumberStyles.Any, CultureInfo.InvariantCulture, out long result);
            return result;
        }

        public static implicit operator ItemIdSet(PropertyValue pv)
        {
            return ItemIdSet.FromString(pv._value);
        }
    }
}