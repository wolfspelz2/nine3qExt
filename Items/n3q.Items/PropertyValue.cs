using System;
using System.Collections.Generic;
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
        string _value;

        public static PropertyValue Empty = new PropertyValue();

        const string JoinSeparator = " ";
        static readonly char[] SplitSeparator = new[] { JoinSeparator[0] };

        public PropertyValue()
        {
            _value = "";
        }

        public PropertyValue(string value) { _value = value; }
        public PropertyValue(long value) { _value = value != 0L ? value.ToString(CultureInfo.InvariantCulture) : ""; }
        public PropertyValue(double value) { _value = value != 0D ? value.ToString(CultureInfo.InvariantCulture) : ""; }
        public PropertyValue(bool value) { _value = value ? "true" : ""; }
        public PropertyValue(ValueList ids) { _value = ids.ToString(); }


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

        public static implicit operator ValueList(PropertyValue pv)
        {
            return ValueList.FromString(pv._value);
        }

        public static implicit operator HashSet<string>(PropertyValue pv)
        {
            var result = new HashSet<string>();
            if (Has.Value(pv._value)) {
                var parts = pv._value.Split(SplitSeparator, StringSplitOptions.RemoveEmptyEntries);
                foreach (string part in parts) {
                    result.Add(part);
                }
            }
            return result;
        }

        public void AddToList(PropertyValue listItem)
        {
            var list = ValueList.FromString(_value);
            if (!list.Contains(listItem)) {
                list.Add(listItem);
                _value = list.ToString();
            }
        }

        public void RemoveFromList(PropertyValue listItem)
        {
            var list = ValueList.FromString(_value);
            if (list.Contains(listItem)) {
                list.Remove(listItem);
                _value = list.ToString();
            }
        }

        public bool IsInList(PropertyValue value)
        {
            var s = value.ToString();
            var idx = _value.IndexOf(s);
            return idx >= 0;
        }

        public static implicit operator PropertyValue(string value) { return new PropertyValue(value); }
        public static implicit operator PropertyValue(long value) { return new PropertyValue(value); }
        public static implicit operator PropertyValue(double value) { return new PropertyValue(value); }
        public static implicit operator PropertyValue(bool value) { return new PropertyValue(value); }
        public static implicit operator PropertyValue(ValueList value) { return new PropertyValue(value); }

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

        public static explicit operator List<object>(PropertyValue v)
        {
            throw new NotImplementedException();
        }

        public static bool AreEquivalent(Pid pid, PropertyValue left, PropertyValue right)
        {
            if (left.ToString() == right.ToString()) {
                return true;
            }
            return false;
        }

        #endregion
    }
}