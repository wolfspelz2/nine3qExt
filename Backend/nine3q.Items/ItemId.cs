using System;

namespace nine3q.Items
{
    [Serializable]
    public class ItemId : IEquatable<ItemId>, IComparable<ItemId>
    {
        public const ItemId NoItem = null;

        long Value = 0;

        public ItemId(long nValue)
        {
            Value = nValue;
        }

        public ItemId(string sValue)
        {
            Value = Convert.ToInt64(sValue);
        }

        public static explicit operator long(ItemId id)
        {
            return id.Value;
        }

        public static bool operator ==(ItemId id1, ItemId id2)
        {
            object o1 = id1;
            object o2 = id2;
            bool bIsNull1 = false;
            bool bIsNull2 = false;

            if (o1 == null) {
                bIsNull1 = true;
            } else {
                if ((long)id1 == 0) {
                    bIsNull1 = true;
                }
            }
            if (o2 == null) {
                bIsNull2 = true;
            } else {
                if ((long)id2 == 0) {
                    bIsNull2 = true;
                }
            }

            if (bIsNull1) {
                if (bIsNull2) {
                    return true;
                } else {
                    return false;
                }
            }

            return id1.Equals(id2);
        }

        public static bool operator !=(ItemId id1, ItemId id2)
        {
            return !(id1 == id2);
        }

        public override bool Equals(object other)
        {
            if (other == null) {
                return false;
            }
            if (other.GetType() == typeof(ItemId)) {
                return ((ItemId)other).Value == Value;
            }
            return false;
        }

        public bool IsValid { get { return Value > 0; } }

        #region Fancy stuff

        public ItemId Clone()
        {
            return new ItemId(Value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        #region IEquatable<ItemId> Members

        public bool Equals(ItemId other)
        {
            object o = other;
            if (o == null) {
                return false;
            }
            return Value.Equals(other.Value);
        }

        #endregion

        #region IComparable<ItemId> Members

        public int CompareTo(ItemId other)
        {
            return Value.CompareTo(other.Value);
        }

        #endregion

        #endregion

    }
}
