using System.Collections.Generic;

namespace n3q.Web
{
    public class ConfigRoot : WebConfig
    {
        public void Load()
        {
            if (RunMode == RunModes.Development) {

                AdminTokens = new List<string> { "Token" };
                WebBaseUrl = "http://localhost:5000/";
                ItemServiceXmppUrl = "xmpp:itemsxmpp.dev.sui.li";

            } else {

                var serverAddress = "localhost";
                WebBaseUrl = $"http://{serverAddress}/";
                ItemServiceXmppUrl = "xmpp:itemsxmpp.weblin.com";

            }
        }
    }
}