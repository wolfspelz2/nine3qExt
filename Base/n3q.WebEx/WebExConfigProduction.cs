namespace n3q.WebEx
{
    class WebExConfigProduction : WebExConfigDefinition
    {
        public void Load()
        {
            ConfigSequence += " " + nameof(WebExConfigProduction);

            XmppUserPasswordSHA1Secret = "3b6f88f2bed0f392";

            // temporary
            XmppDomain = "xmpp.k8s.sui.li";
            XmppServiceUrl = "wss://" + XmppDomain + "/xmpp-websocket";
        }
    }
}