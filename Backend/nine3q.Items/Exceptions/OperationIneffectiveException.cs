using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace nine3q.Items.Exceptions
{
    [Serializable]
    public sealed class OperationIneffectiveException : ItemException
    {
        public OperationIneffectiveException(string inventory, ItemId id, ItemId passiveId, string text) : base(inventory, id, "passive=" + passiveId + ": " + text) { Detail = text; }
        public OperationIneffectiveException(string inventory, ItemId id, string text) : base(inventory, id, text) { Detail = text; }

        public OperationIneffectiveException() { }
        public OperationIneffectiveException(string message) : base(message) { }
        public OperationIneffectiveException(string message, Exception innerException) : base(message, innerException) { }
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        private OperationIneffectiveException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override void GetObjectData(SerializationInfo info, StreamingContext context) { if (info == null) { throw new ArgumentNullException("info"); } base.GetObjectData(info, context); }
    }
}
