namespace n3q.Xmpp
{
    public class XmppConfigDefinition : n3q.Common.ConfigBag
    {
        public string ClusterId = "dev";
        public bool LocalhostClustering = true;
        public string ClusteringAzureTableConnectionString = "UseDevelopmentStorage=true";

        public string ComponentHost = "itemsxmpp.dev.sui.li";
        public string ComponentDomain = "itemsxmpp.dev.sui.li";
        public int ComponentPort = 5280;//5555;
        public string ComponentSecret = "28756a7ff5dce";
    }
}
