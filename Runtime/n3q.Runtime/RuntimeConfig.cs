﻿using ConfigSharp;

namespace n3q.Runtime
{
    public class RuntimeConfig : ConfigBag
    {
        public enum RunModes
        {
            Development,
            Test,
            Staging,
            Production
        }

        public RunModes RunMode =
#if DEBUG
            RunModes.Development;
#else
            RunModes.Production;
#endif

        public string XmppServiceUrl = "wss://xmpp.weblin.sui.li/xmpp-websocket";
        public string XmppDomain = "xmpp.weblin.sui.li";
        public string XmppUserPasswordSHA1Secret = "3b6f88f2bed0f392";
        public string IdentificatorUrlTemplate = "http://runtime.weblin.com/Identity/Generated?avatarUrl={avatarUrl}&nickname={nickname}&digest={digest}&imageUrl={imageUrl}";
        public string AnimationsProxyUrlTemplate = "http://runtime.weblin.com/Avatar/InlineData?url={url}";
        public string AnimationsUrlTemplate = "https://avatar.zweitgeist.com/gif/{id}/config.xml";
    }
}
