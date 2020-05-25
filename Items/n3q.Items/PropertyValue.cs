using System;
using System.Globalization;

namespace n3q.Items
{
    public class PropertyValue
    {
        string _value;

        public PropertyValue(string value)
        {
            _value = value;
        }

        public void Set(string value)
        {
            _value = value;
        }

        public static implicit operator string(PropertyValue pv) { return pv._value.ToString(); }

        public static implicit operator long(PropertyValue pv)
        {
            _ = long.TryParse(pv._value, NumberStyles.Any, CultureInfo.InvariantCulture, out long result);
            return result;
        }
    }
}