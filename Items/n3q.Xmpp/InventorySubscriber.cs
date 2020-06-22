namespace n3q.Xmpp
{
    public partial class Controller
    {
        public class InventorySubscriber
        {
            public readonly string UserId;
            public readonly string ParticipantJid;
            public readonly string ClientJid;

            public InventorySubscriber(string userId, string participantJid, string clientJid)
            {
                UserId = userId;
                ParticipantJid = participantJid;
                ClientJid = clientJid;
            }
        }
    }

}