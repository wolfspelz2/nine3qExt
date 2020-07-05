using System.Collections.Generic;

namespace n3q.Web
{
    public class WebConfigDefinition : n3q.Common.ConfigBag
    {
        public bool UseIntegratedCluster = false;
        public string ClusterId = n3q.Common.Cluster.DevelopmentClusterId;
        public bool LocalhostClustering = n3q.Common.Cluster.DevelopmentLocalhostClustering;
        public string ClusteringAzureTableConnectionString = n3q.Common.Cluster.DevelopmentAzureTableConnectionString;

        public List<string> AdminTokens = new List<string> { };
        public string PayloadHashSecret = "mkTz67tZ4dAhjxgd";
        public string ItemServiceXmppUrl = "xmpp:xmpp-component.example.com";

        public const string DefaultItemWebBaseUrl = "https://item-web.example.com/";
        public string ItemWebBaseUrl = DefaultItemWebBaseUrl;
        public string UnavailableUrl = DefaultItemWebBaseUrl + "Embedded/Account?id={id}";
        public string ItemAppearanceBaseUrl = DefaultItemWebBaseUrl + "images/Items/";
        public string ItemServiceWebApiUrl = DefaultItemWebBaseUrl + "rpc";

        public long MaxRpcRequestSize = 100000;
    }
}