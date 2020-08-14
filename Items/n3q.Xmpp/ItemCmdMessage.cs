using System.Collections.Generic;
using System.Globalization;

namespace n3q.Xmpp
{
    public enum XmppMessageType
    {
        Normal,
        Groupchat,
        PrivateChat,
    }

    public class ItemCmdMessage
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
}
