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

                BaseUrl = "http://localhost:5001/";

            } else {

                XmppServiceUrl = "wss://xmpp.weblin.sui.li/xmpp-websocket";
                XmppDomain = "xmpp.weblin.sui.li";

                BaseUrl = "https://webex.weblin.com/";

            }

            IdentificatorUrlTemplate = BaseUrl + "Identity/Generated?avatarUrl={avatarUrl}&nickname={nickname}&digest={digest}&imageUrl={imageUrl}";
            AnimationsProxyUrlTemplate = BaseUrl + "Avatar/InlineData?url={url}";
            ImageProxyUrlTemplate = BaseUrl + "Avatar/HttpBridge?url={url}";

            AdditionalBaseFolder = System.Environment.GetEnvironmentVariable("N3Q_CONFIG_ROOT") ?? AdditionalBaseFolder;
            if (!string.IsNullOrEmpty(AdditionalBaseFolder)) {
                BaseFolder = AdditionalBaseFolder;
                Include(ConfigFile);
            }
        }
    }
}