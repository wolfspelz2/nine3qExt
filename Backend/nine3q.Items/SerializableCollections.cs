using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Globalization;
using System.Diagnostics.Contracts;

namespace nine3q.Items
{
    [Serializable]
    public class ItemIdSet : HashSet<long>
    {
        const string JoinSeparator = " ";
        static readonly char[] SplitSeparator = new[] { JoinSeparator[0] };

        public ItemIdSet() { }

        public ItemIdSet(string blankSeparatedListOfLong)
        {
            FromString(blankSeparatedListOfLong);
        }

        #region For [Serializable]

        protected ItemIdSet(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override void GetObjectData(SerializationInfo info, StreamingContext context) { base.GetObjectData(info, context); }

        #endregion

        public void FromString(string listOfLong)
        {
            var parts = listOfLong.Split(SplitSeparator, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts) {
                if (long.TryParse(part, out long value)) {
                    Add(value);
                }
            }
        }

        public ItemIdSet Clone()
        {
            var clone = new ItemIdSet();
            foreach (long id in this) {
                clone.Add(id);
            }
            return clone;
        }

        public override string ToString()
        {
            return string.Join(JoinSeparator, this);
        }
    }

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
    public class ItemIdPropertiesCollection : Dictionary<long, PropertySet>
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
    public class ItemIdList : List<long>
    {
    }

    [Serializable]
    public class ItemIdMap : Dictionary<long, long>
    {
        public ItemIdMap(string s)
        {
            Contract.Requires(s != null);
            var pairs = s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in pairs) {
                var kv = pair.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                this.Add(long.Parse(kv[0], CultureInfo.InvariantCulture), long.Parse(kv[1], CultureInfo.InvariantCulture));
            }
        }

        public override string ToString()
        {
            return string.Join(" ", Keys.ToList().ConvertAll(id => $"{id.ToString(CultureInfo.InvariantCulture)}:{this[id].ToString(CultureInfo.InvariantCulture)}"));
        }

        #region For [Serializable]

        public ItemIdMap() { }
        protected ItemIdMap(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override void GetObjectData(SerializationInfo info, StreamingContext context) { base.GetObjectData(info, context); }

        #endregion
    }
}
