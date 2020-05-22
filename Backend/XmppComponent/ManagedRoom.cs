using System;
using System.Collections.Generic;

namespace XmppComponent
{
    public class ManagedRoom
    {
        public readonly string RoomId;
        public readonly List<RoomItem> Items = new List<RoomItem>();

        public ManagedRoom(string roomId)
        {
            RoomId = roomId;
        }
    }
}