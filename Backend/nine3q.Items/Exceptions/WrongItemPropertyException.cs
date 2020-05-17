using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace nine3q.Items.Exceptions
{
    [Serializable]
    public sealed class WrongItemPropertyException : ItemException
    {
        public WrongItemPropertyException(string inventory, long id, Pid pid, string reason) : base(inventory, id, "property=" + pid + ": " + reason) { }

        public WrongItemPropertyException() { }
        public WrongItemPropertyException(string message) : base(message) { }
        public WrongItemPropertyException(string message, Exception innerException) : base(message, innerException) { }
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        private WrongItemPropertyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override void GetObjectData(SerializationInfo info, StreamingContext context) { if (info == null) { throw new ArgumentNullException(nameof(info)); } base.GetObjectData(info, context); }
    }
}
