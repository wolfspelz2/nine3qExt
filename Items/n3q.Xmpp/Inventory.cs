using System.Collections.Generic;

namespace XmppComponent
{
    public partial class Controller
    {
        public class Inventory
        {
            public readonly string UserId;
            public readonly string InventoryItemId;
            public readonly string ParticipantJid;
            public readonly Dictionary<string, InventorySubscriber> Subscribers = new Dictionary<string, InventorySubscriber>();

            public Inventory(string userId, string inventoryItemId, string participantJid)
            {
                UserId = userId;
                InventoryItemId = inventoryItemId;
                ParticipantJid = participantJid;
            }
        }
    }

}