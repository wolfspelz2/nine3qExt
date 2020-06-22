using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace ConfigSharp
{
    internal static class Extensions
    {
        internal static object GetMemberValue(this object obj, string name)
        {
            if (obj == null) { return null; }

            foreach (string part in name.Split('.')) {

                Type type = obj.GetType();
                PropertyInfo pi = type.GetProperty(part);
                if (pi != null) {
                    obj = pi.GetValue(obj, null);
                } else {
                    FieldInfo fi = type.GetField(part);
                    if (fi != null) {
                        obj = fi.GetValue(obj);
                    } else {
                        obj = null;
                    }
                }

            }

            return obj;
        }

        internal static T GetMemberValue<T>(this object obj, string name, T defaultValue)
        {
            object value = GetMemberValue(obj, name);
            if (value == null) {
                return defaultValue;
            }

            // throws InvalidCastException if types are incompatible
            return (T)value;
        }

        internal static bool SetMemberValue(this object obj, string key, string value)
        {
            var parts = key.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 1) {
                var name = parts[0];
                var remainingKey = string.Join(".", parts.ToList().Skip(1));
                var prop = obj.GetType().GetProperty(name);
                if (prop != null) {
                    var member = prop.GetValue(obj, null);
                    return SetMemberValue(member, remainingKey, value);
                }
                var field = obj.GetType().GetField(name);
                if (field != null) {
                    var member = field.GetValue(obj);
                    return SetMemberValue(member, remainingKey, value);
                }
                return false;
            }

            Type varType = null;
            object varValue = null;

            var propInfo = obj.GetType().GetProperty(key);
            var fieldInfo = obj.GetType().GetField(key);
            if (propInfo != null) {
                varType = propInfo.PropertyType;
            } else if (fieldInfo != null) {
                varType = fieldInfo.FieldType;
            }

            if (varType != null) {
                if (varType == typeof(string)) {
                    varValue = value;
                } else if (varType == typeof(int)) {
                    varValue = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                } else if (varType == typeof(long)) {
                    varValue = Convert.ToInt64(value, CultureInfo.InvariantCulture);
                } else if (varType == typeof(float)) {
                    varValue = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                } else if (varType == typeof(double)) {
                    varValue = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                } else if (varType == typeof(bool)) {
                    varValue = value.IsTrue();
                } else if (varType == typeof(DateTime)) {
                    varValue = DateTime.Parse(value, CultureInfo.InvariantCulture);
                }
            }

            if (propInfo != null) {
                propInfo.SetValue(obj, varValue, null);
                return true;
            } else if (fieldInfo != null) {
                fieldInfo.SetValue(obj, varValue);
                return true;
            }

            return false;
        }

        internal static bool IsTrue(this string self)
        {
            string s = self.ToLower(CultureInfo.InvariantCulture);
            return s == "true" || s == "1" || s == "on" || s == "yes" || s == "ja" || s == "oui" || s == "ok" || s == "sure" || s == "yessir" || s == "youbet";
        }
    }
}
