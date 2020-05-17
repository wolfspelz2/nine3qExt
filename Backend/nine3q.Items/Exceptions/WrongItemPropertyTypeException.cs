using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace nine3q.Items.Exceptions
{
    [Serializable]
    public sealed class WrongItemPropertyTypeException : ItemException
    {
        public WrongItemPropertyTypeException(string inventory, long id, Pid pid, Property.Type type) : base(inventory, id, "property=" + pid + " is not type " + type) { }

        public WrongItemPropertyTypeException() { }
        public WrongItemPropertyTypeException(string message) : base(message) { }
        public WrongItemPropertyTypeException(string message, Exception innerException) : base(message, innerException) { }
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        private WrongItemPropertyTypeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override void GetObjectData(SerializationInfo info, StreamingContext context) { if (info == null) { throw new ArgumentNullException(nameof(info)); } base.GetObjectData(info, context); }
    }
}
