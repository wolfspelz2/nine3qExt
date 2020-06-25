using ConfigSharp;

namespace n3q.Runtime
{
    class ConfigRoot : RuntimeConfig
    {
        public void Load()
        {
            if (RunMode == RunModes.Development) {

                XmppServiceUrl = "wss://xmpp.weblin.sui.li/xmpp-websocket";
                XmppDomain = "xmpp.weblin.sui.li";
                XmppUserPasswordSHA1Secret = "3b6f88f2bed0f392";
                IdentificatorUrlTemplate = "http://localhost:5001/Identity/Generated?avatarUrl={avatarUrl}&nickname={nickname}&digest={digest}&imageUrl={imageUrl}";
                AnimationsProxyUrlTemplate = "http://localhost:5001/Avatar/InlineData?url={url}";
                AnimationsUrlTemplate = "https://avatar.zweitgeist.com/gif/{id}/config.xml";

            } else {

                XmppServiceUrl = "wss://xmpp.weblin.sui.li/xmpp-websocket";
                XmppDomain = "xmpp.weblin.sui.li";
                XmppUserPasswordSHA1Secret = "3b6f88f2bed0f392";

                IdentificatorUrlTemplate = "http://runtime.weblin.com/Identity/Generated?avatarUrl={avatarUrl}&nickname={nickname}&digest={digest}&imageUrl={imageUrl}";
                AnimationsProxyUrlTemplate = "http://runtime.weblin.com/Avatar/InlineData?url={url}";
                AnimationsUrlTemplate = "https://avatar.zweitgeist.com/gif/{id}/config.xml";

            }
        }
    }
}