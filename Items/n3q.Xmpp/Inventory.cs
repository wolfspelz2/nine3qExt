using System;
using System.Collections.Generic;

namespace XmppComponent
{
    public class Inventory
    {
        public readonly string InventoryId;
        public readonly Dictionary<string, RoomItem> Items = new Dictionary<string, RoomItem>();
        public readonly List<string> Subscribers= new List<string>();

        public Inventory(string inventoryId)
        {
            InventoryId = inventoryId;
        }

        internal bool AddSubscriber(string subscriberJid)
        {
            if (!Subscribers.Contains(subscriberJid)) {
                Subscribers.Add(subscriberJid);
                return true;
            }
            return false;
        }
    }
}