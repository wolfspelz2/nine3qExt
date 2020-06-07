using System.Collections.Generic;
using System.Globalization;

namespace XmppComponent
{
    public enum XmppMessageType
    {
        Normal,
        Groupchat,
        PrivateChat,
    }
    public enum XmppPresenceType { Available, Unavailable }

    public class XmppMessage
    {
        public XmppMessageType MessageType { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Id { get; set; }
        public Dictionary<string, string> Cmd { get; set; } = new Dictionary<string, string>();

        public string Get(string key, string defaultValue)
        {
            if (Cmd.ContainsKey(key)) {
                return Cmd[key];
            }
            return defaultValue;
        }

        public long Get(string key, long defaultValue)
        {
            if (Cmd.ContainsKey(key)) {
                var s = Cmd[key];
                if (long.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out long value)) {
                    return value;
                }
            }
            return defaultValue;
        }
    }

    public class XmppPresence
    {
        public XmppPresenceType PresenceType { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public Dictionary<string, string> Props { get; set; } = new Dictionary<string, string>();
    }
}
