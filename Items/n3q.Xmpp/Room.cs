using System;
using System.Collections.Generic;

namespace n3q.Xmpp
{
    public class Room
    {
        public readonly string RoomId;
        public readonly Dictionary<string, RoomItem> Items = new Dictionary<string, RoomItem>();

        public Room(string roomId)
        {
            RoomId = roomId;
        }
    }
}