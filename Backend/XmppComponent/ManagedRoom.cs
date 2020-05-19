using System.Collections.Generic;

namespace XmppComponent
{
    internal partial class Controller
    {
        public class ManagedRoom
        {
            public readonly string RoomId;
            public readonly Dictionary<long, RoomItem> Items = new Dictionary<long, RoomItem>();

            public ManagedRoom(string roomId)
            {
                RoomId = roomId;
            }
        }

    }
}