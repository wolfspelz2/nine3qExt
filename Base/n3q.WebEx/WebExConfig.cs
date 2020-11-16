namespace n3q.WebEx
{
    class WebExConfig : WebExConfigDefinition
    {
        public void Load()
        {
            ConfigFile = CurrentFile;
            ConfigSequence += nameof(WebExConfig);

            AdditionalConfigRoot = System.Environment.GetEnvironmentVariable(ConfigRootEnvironmentVariableName) ?? AdditionalConfigRoot;
            if (!string.IsNullOrEmpty(AdditionalConfigRoot)) {
                BaseFolder = AdditionalConfigRoot;
                Include(SetupFile);
            }

            if (Setup == SetupMode.Development) {
                XmppDomain = "xmpp.vulcan.weblin.com";
                XmppServiceUrl = "wss://" + XmppDomain + "/xmpp-websocket";
                XmppUserPasswordSHA1Secret = "3b6f88f2bed0f392";
                ExtensionWebBaseUrl = "http://localhost:5001/";
                ItemWebBaseUrl = "http://localhost:5000/";
            } else if (Setup == SetupMode.Stage) {
                XmppDomain = "xmpp.vulcan.weblin.com";
                XmppServiceUrl = "wss://" + XmppDomain + "/xmpp-websocket";
                ExtensionWebBaseUrl = "https://stage01-webex.vulcan.weblin.com/";
                ItemWebBaseUrl = "https://stage01-webit.vulcan.weblin.com/";
            } else {
                XmppDomain = "xmpp.vulcan.weblin.com";
                XmppServiceUrl = "wss://" + XmppDomain + "/xmpp-websocket";
                ExtensionWebBaseUrl = "https://webex.vulcan.weblin.com/";
                ItemWebBaseUrl = "https://webit.vulcan.weblin.com/";
            }

            IdentificatorUrlTemplate = ExtensionWebBaseUrl + "Identity/Generated?avatarUrl={avatarUrl}&nickname={nickname}&digest={digest}&imageUrl={imageUrl}";
            AnimationsProxyUrlTemplate = ExtensionWebBaseUrl + "Avatar/InlineData?url={url}";
            DataUrlProxyUrlTemplate = ExtensionWebBaseUrl + "Avatar/DataUrl?url={url}";
            ImageProxyUrlTemplate = ExtensionWebBaseUrl + "Avatar/HttpBridge?url={url}";
            ItemConfigUrlTemplate = ItemWebBaseUrl + "Config?id={id}";

            AdditionalConfigRoot = System.Environment.GetEnvironmentVariable(ConfigRootEnvironmentVariableName) ?? AdditionalConfigRoot;
            if (!string.IsNullOrEmpty(AdditionalConfigRoot)) {
                BaseFolder = AdditionalConfigRoot;
                Include(ConfigFile);
            }
        }
    }
}