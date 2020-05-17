using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace nine3q.Items.Exceptions
{
    [Serializable]
    public sealed class ActionNotAvailableException : ItemException
    {
        public ActionNotAvailableException(string inventory, long id, string action) : base(inventory, id, "does not have action=" + action) { Detail = "No action " + action; }

        public ActionNotAvailableException() { }
        public ActionNotAvailableException(string message) : base(message) { }
        public ActionNotAvailableException(string message, Exception innerException) : base(message, innerException) { }
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        private ActionNotAvailableException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override void GetObjectData(SerializationInfo info, StreamingContext context) { if (info == null) { throw new ArgumentNullException(nameof(info)); } base.GetObjectData(info, context); }
    }
}
