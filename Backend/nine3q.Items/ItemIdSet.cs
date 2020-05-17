using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace nine3q.Items
{
    [Serializable]
    public class longSet : HashSet<long>
    {
        const string JoinSeparator = " ";
        static readonly char[] SplitSeparator = new[] { JoinSeparator[0] };

        public longSet() { }

        public longSet(string blankSeparatedListOfLong)
        {
            FromString(blankSeparatedListOfLong);
        }

        #region For [Serializable]

        protected longSet(SerializationInfo info, StreamingContext context) : base(info, context) { }
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

        public longSet Clone()
        {
            var clone = new longSet();
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
}
