using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace nine3q.Items.Exceptions
{
    [Serializable]
    public sealed class MissingActionArgumentException : ItemException
    {
        public MissingActionArgumentException(string inventory, ItemId id, string action, Pid pid) : base(inventory, id, "Action=" + action + " needs argument property=" + pid) { }

        public MissingActionArgumentException() { }
        public MissingActionArgumentException(string message) : base(message) { }
        public MissingActionArgumentException(string message, Exception innerException) : base(message, innerException) { }
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        private MissingActionArgumentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override void GetObjectData(SerializationInfo info, StreamingContext context) { if (info == null) { throw new ArgumentNullException("info"); } base.GetObjectData(info, context); }
    }
}
