using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace nine3q.Items
{
    [Serializable]
    public class PidList : List<Pid>
    {
        public const PidList All = null;

        [NonSerialized]
        public static PidList Public = new PidList { Pid.PublicAccess };
        [NonSerialized]
        public static PidList Owner = new PidList { Pid.OwnerAccess };

        //public PidList Clone()
        //{
        //    var clone = new PidList();
        //    foreach (var pid in this) {
        //        clone.Add(pid);
        //    }
        //    return clone;
        //}
    }

    [Serializable]
    public class ItemIdPropertiesCollection : Dictionary<ItemId, PropertySet>
    {
        #region For [Serializable]

        public ItemIdPropertiesCollection() { }
        protected ItemIdPropertiesCollection(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override void GetObjectData(SerializationInfo info, StreamingContext context) { base.GetObjectData(info, context); }

        #endregion
    }

    [Serializable]
    public class NamePropertiesCollection : Dictionary<string, PropertySet>
    {
        #region For [Serializable]

        public NamePropertiesCollection() { }
        protected NamePropertiesCollection(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override void GetObjectData(SerializationInfo info, StreamingContext context) { base.GetObjectData(info, context); }

        #endregion
    }

    [Serializable]
    public class ItemIdList : List<ItemId>
    {
    }

    [Serializable]
    public class ItemIdMap : Dictionary<ItemId, ItemId>
    {
        public ItemIdMap(string s)
        {
            var pairs = s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in pairs) {
                var kv = pair.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                this.Add(new ItemId(kv[0]), new ItemId(kv[1]));
            }
        }

        public override string ToString()
        {
            return string.Join(" ", Keys.ToList().ConvertAll(id => $"{id.ToString()}:{this[id].ToString()}"));
        }

        #region For [Serializable]

        public ItemIdMap() { }
        protected ItemIdMap(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override void GetObjectData(SerializationInfo info, StreamingContext context) { base.GetObjectData(info, context); }

        #endregion
    }
}
