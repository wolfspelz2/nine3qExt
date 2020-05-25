using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace XmppComponent
{
    [Serializable]
    public sealed class SurfaceException : Exception
    {
        public string Context { get; set; }
        public long ItemId;
        public SurfaceNotification.Fact Fact { get; set; }
        public SurfaceNotification.Reason Reason { get; set; }

        public SurfaceException(string contextId, long itemId, SurfaceNotification.Fact fact, SurfaceNotification.Reason reason)
            : base($"{contextId} item {itemId}: {fact.ToString()} {reason.ToString()}")
        {
            Context = contextId;
            ItemId = itemId;
            Fact = fact;
            Reason = reason;
        }

        public SurfaceException() { }
        public SurfaceException(string message) : base(message) { }
        public SurfaceException(string message, Exception innerException) : base(message, innerException) { }
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        private SurfaceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override void GetObjectData(SerializationInfo info, StreamingContext context) { if (info == null) { throw new ArgumentNullException("info"); } base.GetObjectData(info, context); }
    }

    public class SurfaceNotification
    {
        public enum Fact
        {
            NoError = 0,
            Error,
            NotRezzed,
            NotDerezzed,
        }

        public enum Reason
        {
            ItemNotRezable,
            ItemNotRezzed,
            NotYourItem,
            TransferFailed,
        }
    }
}
