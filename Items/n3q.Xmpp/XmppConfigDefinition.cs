namespace n3q.Xmpp
{
    public class XmppConfigDefinition : n3q.Common.ConfigBag
    {
        public string ClusterId = n3q.Common.Cluster.DevelopmentClusterId;
        public bool LocalhostClustering = n3q.Common.Cluster.DevelopmentLocalhostClustering;
        public string ClusteringAzureTableConnectionString = "UseDevelopmentStorage=true";

        public string ComponentHost = "itemsxmpp.dev.sui.li";
        public string ComponentDomain = "itemsxmpp.dev.sui.li";
        public int ComponentPort = 5347;//5555;
        public string ComponentSecret = "28756a7ff5dce";
    }
}
