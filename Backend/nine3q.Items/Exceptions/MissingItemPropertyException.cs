using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace nine3q.Items.Exceptions
{
    [Serializable]
    public sealed class MissingItemPropertyException : ItemException
    {
        public MissingItemPropertyException(string inventory, ItemId id, Pid pid) : base(inventory, id, "needs property=" + pid + " for this operation") { }

        public MissingItemPropertyException() { }
        public MissingItemPropertyException(string message) : base(message) { }
        public MissingItemPropertyException(string message, Exception innerException) : base(message, innerException) { }
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        private MissingItemPropertyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override void GetObjectData(SerializationInfo info, StreamingContext context) { if (info == null) { throw new ArgumentNullException("info"); } base.GetObjectData(info, context); }
    }
}
