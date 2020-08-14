using System.Collections.Generic;

namespace n3q.Xmpp
{
    public enum XmppPresenceType
    {
        Available,
        Unavailable,
        Error
    }

    public class ItemPropsPresence
    {
        public XmppPresenceType PresenceType { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public Dictionary<string, string> Props { get; set; } = new Dictionary<string, string>();
    }
}
