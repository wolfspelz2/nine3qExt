using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using n3q.Tools;

namespace n3q.Items
{
    //[Serializable]
    public class PropertySet : Dictionary<Pid, PropertyValue>
    {
        public PropertySet()
        {
        }

        public PropertySet(Dictionary<Pid, string> properties)
        {
            if (properties != null) {
                _ = properties.Select(pair => this[pair.Key] = new PropertyValue(pair.Value));
            }
        }

        public PropertyValue Get(Pid pid)
        {
            if (ContainsKey(pid)) {
                return this[pid];
            }
            return new PropertyValue();
        }

        public void Set(Pid pid, string value)
        {
            this[pid] = new PropertyValue(value);
        }

        public void Set(Pid pid, long value)
        {
            this[pid] = new PropertyValue(value);
        }

        public void Set(Pid pid, double value)
        {
            this[pid] = new PropertyValue(value);
        }

        public void Set(Pid pid, bool value)
        {
            this[pid] = new PropertyValue(value);
        }

        public void Set(Pid pid, ItemIdSet ids)
        {
            this[pid] = new PropertyValue(ids);
        }

        //protected PropertySet(SerializationInfo serializationInfo, StreamingContext streamingContext)
        //{
        //    throw new NotImplementedException();
        //}

        //#region Serializable

        //const string KeyNamesAttribute = "__Properties";
        //const string AttributePrefix = "_";
        //const string Separator = " ";
        //static readonly char[] SeparatorSplitArg = new[] { Separator[0] };

        //protected PropertySet(SerializationInfo info, StreamingContext context)
        //{
        //    Contract.Requires(info != null);
        //    var propertyNames = info.GetString(KeyNamesAttribute).Split(SeparatorSplitArg, StringSplitOptions.RemoveEmptyEntries);
        //    foreach (var name in propertyNames) {
        //        var attr = AttributePrefix + name;
        //        Add(name.ToEnum(Pid.Unknown), new PropertyValue(info.GetString(attr)));
        //    }
        //}

        //public override void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    Contract.Requires(info != null);
        //    base.GetObjectData(info, context);
        //    foreach (var pair in this) {
        //        info.AddValue(AttributePrefix + pair.Key.ToString(), pair.Value.ToString());
        //    }
        //    info.AddValue(KeyNamesAttribute, string.Join(Separator, Keys.ToList().ConvertAll(x => x.ToString())));
        //}

        //#endregion
    }
}
