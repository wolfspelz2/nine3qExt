namespace n3q.WebEx
{
    class WebExConfig : WebExConfigDefinition
    {
        public void Load()
        {
            ConfigFile = CurrentFile;
            ConfigSequence += nameof(WebExConfig);

        //public string XmppServiceUrl = "wss://xmpp-user-host.example.com/xmpp-websocket";
        //public string XmppDomain = "xmpp-user-host.example.com";
        //public string XmppUserPasswordSHA1Secret = "3b6f88f2bed0f392";

        //public const string DefaultBaseUrl = "https://ex-web.example.com/";


            if (Build == BuildConfiguration.Debug) {

                //XmppDomain = "xmpp.weblin.sui.li";
                XmppDomain = "xmpp.k8s.sui.li";
                XmppServiceUrl = "wss://" + XmppDomain + "/xmpp-websocket";
                XmppUserPasswordSHA1Secret = "3b6f88f2bed0f392";

                BaseUrl = "http://localhost:5001/";

            } else {

                //XmppDomain = "xmpp.weblin.sui.li";
                XmppDomain = "xmpp.k8s.sui.li";
                XmppServiceUrl = "wss://" + XmppDomain + "/xmpp-websocket";

                BaseUrl = "https://n3qwebex.k8s.sui.li/";

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