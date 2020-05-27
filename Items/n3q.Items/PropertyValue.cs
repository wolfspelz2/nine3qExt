using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using n3q.Tools;

namespace n3q.Items
{
    //[DataContract]
    public class PropertyValue
    {
        //[DataMember]
        readonly string _value;

        public static PropertyValue Empty = new PropertyValue();

        public PropertyValue()
        {
            _value = "";
        }

        public PropertyValue(string value) { _value = value; }
        public PropertyValue(long value) { _value = value != 0L ? value.ToString(CultureInfo.InvariantCulture) : ""; }
        public PropertyValue(double value) { _value = value != 0D ? value.ToString(CultureInfo.InvariantCulture) : ""; }
        public PropertyValue(bool value) { _value = value ? "true" : ""; }
        public PropertyValue(ItemIdSet ids) { _value = ids.ToString(); }

        public static implicit operator string(PropertyValue pv)
        {
            return pv._value;
        }

        public static implicit operator long(PropertyValue pv)
        {
            if (long.TryParse(pv._value, NumberStyles.Any, CultureInfo.InvariantCulture, out long result)) {
                return result;
            } else {
                return 0L;
            }
        }

        public static implicit operator double(PropertyValue pv)
        {
            if (double.TryParse(pv._value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result)) {
                return result;
            } else {
                return 0.0D;
            }
        }

        public static implicit operator bool(PropertyValue pv)
        {
            return pv._value.IsTrue();
        }

        public static implicit operator ItemIdSet(PropertyValue pv)
        {
            return ItemIdSet.FromString(pv._value);
        }

        public static implicit operator PropertyValue(string value) { return new PropertyValue(value); }
        public static implicit operator PropertyValue(long value) { return new PropertyValue(value); }
        public static implicit operator PropertyValue(double value) { return new PropertyValue(value); }
        public static implicit operator PropertyValue(bool value) { return new PropertyValue(value); }
        public static implicit operator PropertyValue(ItemIdSet value) { return new PropertyValue(value); }

        public override string ToString()
        {
            return _value;
        }

        #region Defined values

        public enum Roles
        {
            Public,
            User,
            PowerUser,
            Moderator,
            LeadModerator,
            Janitor,
            LeadJanitor,
            Content,
            LeadContent,
            Admin,
            Developer,
            SecurityAdmin
        }

        public enum TestEnum
        {
            Unknown,
            Value1,
            Value2,
        }

        public static bool AreEquivalent(Pid pid, PropertyValue left, PropertyValue right)
        {
            if (left.ToString() == right.ToString()) {
                return true;
            }
            return false;
        }

        public static PropertyValue Default(Pid pid)
        {
            var type = Property.GetDefinition(pid).Type;
            return type switch
            {
                Property.Type.Unknown => throw new InvalidOperationException("Property type=" + type.ToString() + " should not never surface."),
                Property.Type.Int => 0L,
                Property.Type.String => "",
                Property.Type.Float => 0.0D,
                Property.Type.Bool => false,
                Property.Type.ItemSet => new ItemIdSet(),
                _ => throw new NotImplementedException("Property type=" + type.ToString() + " not yet implemented."),
            };
        }

        #endregion
    }
}