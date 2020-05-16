using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using nine3q.Lib;

namespace nine3q.Items
{
    [Serializable]
    public class PropertySet : Dictionary<Pid, object>
    {
        public PropertySet() { }

        public void Set(Pid key, object value)
        {
            this[key] = Property.Normalize(Property.Get(key).Type, value);
        }

        public object Get_maybe_null(Pid key)
        {
            object value = null;
            TryGetValue(key, out value);
            return value;
        }

        public void Normalize()
        {
            // Change values of Dictionary in-place
            // Don't copy, becaue copying would break ItemAspect
            foreach (var key in this.Keys.ToList()) {
                this[key] = Property.Normalize(Property.Get(key).Type, this[key]);
            }
        }

        public void Delete(Pid pid)
        {
            if (ContainsKey(pid)) {
                Remove(pid);
            }
        }

        public void AddToItemSet(Pid pid, ItemId id)
        {
            var list = GetItemSet(pid);
            if (list.Count == 0) {
                list.Add(id);
                Set(pid, list);
            } else {
                list.Add(id);
            }
        }

        public void RemoveFromItemSet(Pid pid, ItemId id)
        {
            var list = GetItemSet(pid).Remove(id);
        }

        public long GetInt(Pid key, long defaultValue)
        {
            if (!ContainsKey(key)) { return defaultValue; }
            return GetInt(key);
        }
        public long GetInt(Pid key)
        {
            if (!ContainsKey(key)) { return (long)Property.Default(Property.Type.Int); }
            var value = this[key];
            if (value is int) return (int)value;
            return (long)value;
        }

        public string GetString(Pid key, string defaultValue)
        {
            if (!ContainsKey(key)) { return defaultValue; }
            return GetString(key);
        }
        public string GetString(Pid key)
        {
            if (!ContainsKey(key)) { return (string)Property.Default(Property.Type.String); }
            var value = this[key];
            if (value is Pid) return ((Pid)value).ToString();
            if (value is string) return (string)value;
            return value.ToString();
        }

        public double GetFloat(Pid key, double defaultValue)
        {
            if (!ContainsKey(key)) { return defaultValue; }
            return GetFloat(key);
        }
        public double GetFloat(Pid key)
        {
            if (!ContainsKey(key)) { return (double)Property.Default(Property.Type.Float); }
            var value = this[key];
            if (value is int) return (float)value;
            return (double)value;
        }

        public bool GetBool(Pid key, bool defaultValue)
        {
            if (!ContainsKey(key)) { return defaultValue; }
            return GetBool(key);
        }
        public bool GetBool(Pid key)
        {
            if (!ContainsKey(key)) { return (bool)Property.Default(Property.Type.Bool); }
            return (bool)this[key];
        }

        public ItemId GetItem(Pid key)
        {
            if (!ContainsKey(key)) { return Property.Default(Property.Type.Item) as ItemId; }
            var value = this[key];
            return (ItemId)value;
        }

        public ItemIdSet GetItemSet(Pid key)
        {
            if (!ContainsKey(key)) { return Property.Default(Property.Type.ItemSet) as ItemIdSet; }
            return (ItemIdSet)this[key];
        }

        public T GetEnum<T>(Pid pid, T defaultValue) where T : struct
        {
            var value = GetString(pid);
            if (!string.IsNullOrEmpty(value)) {
                T parsed;
                if (Enum.TryParse(value, out parsed)) {
                    return parsed;
                }
            }
            return defaultValue;
        }

        public override string ToString()
        {
            return string.Join(" ", this.Select(pair => $"{pair.Key}={pair.Value}"));
            //var sb = new StringBuilder();
            //foreach (var pair in this) {
            //    sb.Append(pair.Key.ToString());
            //    sb.Append("=");
            //    sb.Append(pair.Value);
            //    sb.Append(" ");
            //}
            //return sb.ToString();
        }

        #region For [Serializable]

        const string KeyNamesAttribute = "__Properties";
        const string AttributePrefix = "_";
        const string Separator = " ";
        static char[] SeparatorSplitArg = new[] { Separator[0] };

        protected PropertySet(SerializationInfo info, StreamingContext context)
        {
            var propertyNames = info.GetString(KeyNamesAttribute).Split(SeparatorSplitArg, StringSplitOptions.RemoveEmptyEntries);
            foreach (var name in propertyNames) {
                var prop = Property.Get(name);
                var attr = AttributePrefix + name;
                if (prop.Type == Property.Type.Int) {
                    Add(prop.Id, info.GetInt64(attr));
                } else if (prop.Type == Property.Type.String) {
                    Add(prop.Id, info.GetString(attr));
                } else if (prop.Type == Property.Type.Float) {
                    Add(prop.Id, info.GetDouble(attr));
                } else if (prop.Type == Property.Type.Bool) {
                    Add(prop.Id, info.GetString(attr).IsTrue());
                } else if (prop.Type == Property.Type.Item) {
                    Add(prop.Id, new ItemId(info.GetInt64(attr)));
                } else if (prop.Type == Property.Type.ItemSet) {
                    Add(prop.Id, new ItemIdSet(info.GetString(attr)));
                } else {
                    throw new NotImplementedException("Property name=" + name + " not yet implemented.");
                }
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            foreach (var pair in this) {
                var prop = Property.Get(pair.Key);
                info.AddValue(AttributePrefix + prop.Name, Property.ToString(prop.Type, pair.Value));
            }
            info.AddValue(KeyNamesAttribute, string.Join(Separator, Keys.ToList().ConvertAll(x => x.ToString())));
        }

        #endregion
    }
}
