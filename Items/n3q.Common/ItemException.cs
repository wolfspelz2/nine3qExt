using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace n3q.Common
{
    [Serializable]
    public sealed class ItemException : Exception
    {
        public string ItemId { get; set; }
        public string OtherId;

        public ItemNotification.Fact Fact { get; set; }
        public ItemNotification.Reason Reason { get; set; }

        public ItemException(string firstItemId, string otherItemId, ItemNotification.Fact fact, ItemNotification.Reason reason)
            : base($"{firstItemId} and {otherItemId}: {fact.ToString()} {reason.ToString()}")
        {
            ItemId = firstItemId;
            OtherId = otherItemId;
            Fact = fact;
            Reason = reason;
        }

        public ItemException(string itemId, ItemNotification.Fact fact, ItemNotification.Reason reason)
            : base($"{itemId}: {fact.ToString()} {reason.ToString()}")
        {
            ItemId = itemId;
            Fact = fact;
            Reason = reason;
        }

        public ItemException(ItemNotification.Fact fact, ItemNotification.Reason reason)
            : base($"{fact.ToString()} {reason.ToString()}")
        {
            Fact = fact;
            Reason = reason;
        }

        public ItemException() { }
        public ItemException(string message) : base(message) { }
        public ItemException(string message, Exception innerException) : base(message, innerException) { }
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        private ItemException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) { if (info == null) { throw new ArgumentNullException(nameof(info)); } base.GetObjectData(info, context); }
    }

    public static class ItemNotification
    {
        public enum Fact
        {
            NoError = 0,
            Error,
            NotExecuted,
            NotRezzed,
            NotDerezzed,
            NotMoved,
            NotCreated,
        }

        public enum Reason
        {
            NoReason = 0,
            ItemIsNotRezable,
            ItemIsNotRezzed,
            NotYourItem,
            ItemCapacityLimit,
            ServiceUnavailable,
            ItemIsNotMovable,
            ItemDepleted,
            IdenticalItems,
            StillInCooldown,
        }
    }
}
