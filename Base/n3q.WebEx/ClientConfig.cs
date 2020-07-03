using System.Collections.Generic;

namespace n3q.WebEx
{
    public class XmppConfig
    {
        public string service { get; set; }
        public string domain { get; set; }
        public string user { get; set; }
        public string pass { get; set; }
    }

    public class IdentityConfig
    {
        public string identificatorUrlTemplate { get; set; }
    }

    public class ItemProviderConfig
    {
        public string name { get; set; }
        public string description { get; set; }
        public string configUrl { get; set; }
    }

    public class ItemProviderConfigSet : Dictionary<string, ItemProviderConfig>
    {
    }

    public class AvatarsConfig
    {
        public string animationsUrlTemplate { get; set; }
        public string animationsProxyUrlTemplate { get; set; }
        public List<string> list { get; set; }
    }

    public class ClientConfig
    {
        public XmppConfig xmpp { get; set; } = new XmppConfig();
        public ItemProviderConfigSet itemProviders { get; set; } = new ItemProviderConfigSet();
        public IdentityConfig identity { get; set; } = new IdentityConfig();
        public AvatarsConfig avatars { get; set; } = new AvatarsConfig();

    }
}
