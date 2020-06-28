using System.Collections.Generic;

namespace n3q.Web
{
    public class WebConfigRoot : WebConfig
    {
        public void Load()
        {
            ConfigSequence += nameof(WebConfigRoot);
            if (RunMode == RunModes.Development) {

                ClusterId = "dev";
                LocalhostClustering = false;
                ClusteringAzureTableConnectionString = "DefaultEndpointsProtocol=https;AccountName=nine3qstoragetest;AccountKey=4Ov/kZAXYi4seMphX/t6jyTmvOuXVqf8P0M5QHd3b+mpHWJOzvo5gED9H23R4hMzxhMNueXoRyW4rk4BCctRuQ==;EndpointSuffix=core.windows.net";

                AdminTokens = new List<string> { "Token" };
                ItemServiceXmppUrl = "xmpp:itemsxmpp.dev.sui.li";
                UnavailableUrl = "http://localhost:5000/Embedded/Account?id={id}";
                ItemBaseValue = "http://localhost:5000/images/Items/";

            } else {

                ClusterId = "prod";
                LocalhostClustering = false;

                AdminTokens = new List<string>();
                ItemServiceXmppUrl = "xmpp:itemsxmpp.weblin.com";
                UnavailableUrl = "https://itemsweb.weblin.com/Embedded/Account?id={id}";
                ItemBaseValue = "https://itemsweb.weblin.com/images/Items/";

                Include("WebConfigProduction.cs");

            }
        }
    }
}