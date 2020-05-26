using System;
using System.Collections.Generic;

namespace XmppComponent
{
    public class ManagedRoom
    {
        public readonly string RoomId;
        public readonly Dictionary<string, RoomItem> Items = new Dictionary<string, RoomItem>();

        public ManagedRoom(string roomId)
        {
            RoomId = roomId;
        }
    }
}