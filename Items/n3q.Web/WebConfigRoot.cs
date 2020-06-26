using System.Collections.Generic;

namespace n3q.Web
{
    public class WebConfigRoot : WebConfig
    {
        public void Load()
        {
            ConfigSequence += "WebConfigRoot";
            if (RunMode == RunModes.Development) {

                AdminTokens = new List<string> { "Token" };

                ItemServiceXmppUrl = "xmpp:itemsxmpp.dev.sui.li";
                UnavailableUrl = "http://localhost:5000/Embedded/Account?id={id}";
                ItemBaseUrl = "http://localhost:5000/images/Items/";

            } else {

                Include("WebConfigProduction.cs");

                ItemServiceXmppUrl = "xmpp:itemsxmpp.weblin.com";
                UnavailableUrl = "https://items.weblin.com/Embedded/Account?id={id}";
                ItemBaseUrl = "https://items.weblin.com/images/Items/";

            }
        }
    }
}