using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace nine3q.Items.Exceptions
{
    [Serializable]
    public sealed class JsonConfigPropertyFormatException : ItemException
    {
        public JsonConfigPropertyFormatException(string inventory, long id, Pid pid, string text) : base(inventory, id, "invalid format of property=" + pid + ": " + text) { Detail = text; }

        public JsonConfigPropertyFormatException() { }
        public JsonConfigPropertyFormatException(string message) : base(message) { }
        public JsonConfigPropertyFormatException(string message, Exception innerException) : base(message, innerException) { }
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        private JsonConfigPropertyFormatException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override void GetObjectData(SerializationInfo info, StreamingContext context) { if (info == null) { throw new ArgumentNullException(nameof(info)); } base.GetObjectData(info, context); }
    }
}
