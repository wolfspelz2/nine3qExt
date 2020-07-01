namespace n3q.Xmpp
{
    public class XmppConfigDefinition : n3q.Common.ConfigBag
    {
        public string ClusterId = n3q.Common.Cluster.DevelopmentClusterId;
        public bool LocalhostClustering = n3q.Common.Cluster.DevelopmentLocalhostClustering;
        public string ClusteringAzureTableConnectionString = "UseDevelopmentStorage=true";

        public string ComponentHost = "xmpp-component-host.example.com";
        public int ComponentPort = 5347;//5555;
        public string ComponentDomain = "xmpp-component.example.com";
        public string ComponentSecret = "28756a7ff5dce";

        public int ClusterConnectSecondsBetweenRetries = 4;
        public int XmppConnectSecondsBetweenRetries = 4;
    }
}
