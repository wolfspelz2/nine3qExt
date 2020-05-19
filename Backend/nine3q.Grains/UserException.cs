using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace nine3q.Grains
{
    [Serializable]
    public sealed class ProtocolException : Exception
    {
        public string Context { get; set; }
        public SurfaceNotification.Fact Code { get; set; }

        public ProtocolException(string context, SurfaceNotification.Fact code, string text) : base(text) { Context = context; Code = code; }

        public ProtocolException() { }
        public ProtocolException(string message) : base(message) { }
        public ProtocolException(string message, Exception innerException) : base(message, innerException) { }
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        private ProtocolException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public override void GetObjectData(SerializationInfo info, StreamingContext context) { if (info == null) { throw new ArgumentNullException("info"); } base.GetObjectData(info, context); }
    }
}
