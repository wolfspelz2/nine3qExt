using System.Collections.Generic;

namespace n3q.WebEx
{
    public class WebExConfigDefinition : n3q.Common.ConfigBag
    {
        public string XmppServiceUrl = "wss://xmpp.weblin.com/xmpp-websocket";
        public string XmppDomain = "xmpp.weblin.weblin.com";
        public string XmppUserPasswordSHA1Secret = "3b6f88f2bed0f392";

        public string IdentificatorUrlTemplate = "https://webex.weblin.com/Identity/Generated?avatarUrl={avatarUrl}&nickname={nickname}&digest={digest}&imageUrl={imageUrl}";
        public string AnimationsProxyUrlTemplate = "https://webex.weblin.com/Avatar/InlineData?url={url}";
        public string AnimationsUrlTemplate = "https://avatar.zweitgeist.com/gif/{id}/config.xml";
        public List<string> AvatarProxyPreloadSequenceNames = new List<string> { "idle", "moveright", "moveleft" };
        public long MemoryCacheSizeBytes = 200 * 1024 * 1024;
        public bool UpgradeAvatarUrlToHttps = true;
    }
}
