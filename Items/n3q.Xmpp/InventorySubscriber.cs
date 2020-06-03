namespace XmppComponent
{
    public partial class Controller
    {
        public class InventorySubscriber
        {
            public readonly string UserId;
            public readonly string ClientJid;

            public InventorySubscriber(string userId, string clientJid)
            {
                UserId = userId;
                ClientJid = clientJid;
            }
        }
    }

}