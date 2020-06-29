namespace n3q.WebEx
{
    class WebExConfig : WebExConfigDefinition
    {
        public void Load()
        {
            ConfigFile = CurrentFile;
            ConfigSequence += nameof(WebExConfig);

            if (Build == BuildConfiguration.Debug) {

                XmppServiceUrl = "wss://xmpp.weblin.sui.li/xmpp-websocket";
                XmppDomain = "xmpp.weblin.sui.li";
                XmppUserPasswordSHA1Secret = "3b6f88f2bed0f392";

                IdentificatorUrlTemplate = "http://localhost:5001/Identity/Generated?avatarUrl={avatarUrl}&nickname={nickname}&digest={digest}&imageUrl={imageUrl}";
                AnimationsProxyUrlTemplate = "http://localhost:5001/Avatar/InlineData?url={url}";
                AnimationsUrlTemplate = "https://avatar.zweitgeist.com/gif/{id}/config.xml";

            } else {

                XmppServiceUrl = "wss://xmpp.weblin.sui.li/xmpp-websocket";
                XmppDomain = "xmpp.weblin.sui.li";

                IdentificatorUrlTemplate = "https://webex.weblin.com/Identity/Generated?avatarUrl={avatarUrl}&nickname={nickname}&digest={digest}&imageUrl={imageUrl}";
                AnimationsProxyUrlTemplate = "https://webex.weblin.com/Avatar/InlineData?url={url}";
                AnimationsUrlTemplate = "https://avatar.zweitgeist.com/gif/{id}/config.xml";

            }

            AdditionalBaseFolder = System.Environment.GetEnvironmentVariable("N3Q_CONFIG_ROOT") ?? AdditionalBaseFolder;
            if (!string.IsNullOrEmpty(AdditionalBaseFolder)) {
                BaseFolder = AdditionalBaseFolder;
                Include(ConfigFile);
            }
        }
    }
}