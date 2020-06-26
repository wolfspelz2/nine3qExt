using System.Collections.Generic;
using ConfigSharp;

namespace n3q.Web
{
    public class WebConfig : ConfigBag
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

        public string ConfigSequence = "";

        public bool UseIntegratedCluster = false;
        public bool LocalhostClustering = true;
        public string ClusteringAzureTableConnectionString = "UseDevelopmentStorage=true";
        public string ClusterId = "dev";

        public List<string> AdminTokens = new List<string> { };
        public string ItemServiceXmppUrl = "xmpp:itemsxmpp.weblin.com";
        public string UnavailableUrl = "https://items.weblin.com/Embedded/Account?id={id}";
        public string ItemBaseUrl = "https://items.weblin.com/images/Items/";
    }
}