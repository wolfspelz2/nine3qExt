using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace nine3q.Items.Exceptions
{
    [Serializable]
    public class ItemException : Exception
    {
        public string Detail { get; set; } = "";

        public ItemException(string inventory, long id, string message) : base(inventory + " item=" + id + ": " + message) { }

        public ItemException() { }
        public ItemException(string message) : base(message) { }
        public ItemException(string message, Exception innerException) : base(message, innerException) { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected ItemException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Detail = info.GetString("_Detail");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null) { throw new ArgumentNullException(nameof(info)); }
            base.GetObjectData(info, context);
            info.AddValue("_Detail", Detail);
        }
    }
}
