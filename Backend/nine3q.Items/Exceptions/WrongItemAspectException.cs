using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace nine3q.Items.Exceptions
{
    [Serializable]
    public sealed class WrongItemAspectException : ItemException
    {
        public WrongItemAspectException(string inventory, ItemId id, Pid pid) : base(inventory, id, "is not property=" + pid) { }

        public WrongItemAspectException() { }
        public WrongItemAspectException(string message) : base(message) { }
        public WrongItemAspectException(string message, Exception innerException) : base(message, innerException) { }
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        private WrongItemAspectException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override void GetObjectData(SerializationInfo info, StreamingContext context) { if (info == null) { throw new ArgumentNullException("info"); } base.GetObjectData(info, context); }
    }
}
