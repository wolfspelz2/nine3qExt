using System.Collections.Generic;

namespace n3q.Web
{
    public class WebConfigDefinition : n3q.Common.ConfigBag
    {
        public bool UseIntegratedCluster = false;
        public bool LocalhostClustering = true;
        public string ClusteringAzureTableConnectionString = "UseDevelopmentStorage=true";
        public string ClusterId = "dev";

        public List<string> AdminTokens = new List<string> { };
        public string ItemServiceXmppUrl = "xmpp:itemsxmpp.weblin.com";
        public string UnavailableUrl = "https://itemsweb.weblin.com/Embedded/Account?id={id}";
        public string ItemBaseUrl = "https://itemsweb.weblin.com/images/Items/";
    }
}