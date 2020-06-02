using System.Collections.Generic;

namespace XmppComponent
{
    public enum XmppMessageType { Normal, Groupchat }
    public enum XmppPresenceType { Available, Unavailable }

    public class XmppMessage
    {
        public XmppMessageType MessageType { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Id { get; set; }
        public Dictionary<string, string> Cmd { get; set; } = new Dictionary<string, string>();
    }

    public class XmppPresence
    {
        public XmppPresenceType PresenceType { get; set; }
        public string From { get; set; }
        public Dictionary<string, string> Props { get; set; } = new Dictionary<string, string>();
    }
}
