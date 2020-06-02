using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace n3q.Common
{
    [Serializable]
    public sealed class SurfaceException : Exception
    {
        public string Context { get; set; }
        public string ItemId;

        public SurfaceNotification.Fact Fact { get; set; }
        public SurfaceNotification.Reason Reason { get; set; }

        public SurfaceException(string contextId, string itemId, SurfaceNotification.Fact fact, SurfaceNotification.Reason reason)
            : base($"{contextId} item {itemId}: {fact.ToString()} {reason.ToString()}")
        {
            Context = contextId;
            ItemId = itemId;
            Fact = fact;
            Reason = reason;
        }

        public SurfaceException(SurfaceNotification.Fact fact, SurfaceNotification.Reason reason)
            : base($"{fact.ToString()} {reason.ToString()}")
        {
            Fact = fact;
            Reason = reason;
        }

        public SurfaceException() { }
        public SurfaceException(string message) : base(message) { }
        public SurfaceException(string message, Exception innerException) : base(message, innerException) { }
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        private SurfaceException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) { if (info == null) { throw new ArgumentNullException(nameof(info)); } base.GetObjectData(info, context); }
    }

    public static class SurfaceNotification
    {
        public enum Fact
        {
            NoError = 0,
            Error,
            NotExecuted,
            NotRezzed,
            NotDerezzed,
        }

        public enum Reason
        {
            ItemIsNotRezable,
            ItemIsNotRezzed,
            NotYourItem,
            ItemCapacityLimit,
            ServiceUnavailable,
        }
    }
}
