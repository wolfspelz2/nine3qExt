using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace nine3q.Items.Exceptions
{
    [Serializable]
    public sealed class SlotAvailabilityException : ItemException
    {
        public SlotAvailabilityException(string inventory, long id, long childId, string text) : base(inventory, id, "No slot for " + childId + ": " + text) { }

        public SlotAvailabilityException() { }
        public SlotAvailabilityException(string message) : base(message) { }
        public SlotAvailabilityException(string message, Exception innerException) : base(message, innerException) { }
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        private SlotAvailabilityException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override void GetObjectData(SerializationInfo info, StreamingContext context) { if (info == null) { throw new ArgumentNullException(paramName: nameof(info)); } base.GetObjectData(info, context); }
    }
}
