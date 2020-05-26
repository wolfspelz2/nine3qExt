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

        public PropertyValue()
        {
            _value = "";
        }

        public PropertyValue(string value) { _value = value; }
        public PropertyValue(long value) { _value = value.ToString(CultureInfo.InvariantCulture); }
        public PropertyValue(double value) { _value = value.ToString(CultureInfo.InvariantCulture); }
        public PropertyValue(bool value) { _value = value.ToString(CultureInfo.InvariantCulture); }
        public PropertyValue(ItemIdSet ids) { _value = ids.ToString(); }

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

        #endregion
    }
}