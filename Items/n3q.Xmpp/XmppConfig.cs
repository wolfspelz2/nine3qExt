namespace n3q.Xmpp
{
    public class XmppConfig : ConfigSharp.ConfigBag
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
        public string ConfigFile = "XmppConfigRoot.cs";

        public string ClusterId = "dev";
        public bool LocalhostClustering = true;
        public string ClusteringAzureTableConnectionString = "UseDevelopmentStorage=true";

        public string ComponentHost = "itemsxmpp.dev.sui.li";
        public string ComponentDomain = "itemsxmpp.dev.sui.li";
        public int ComponentPort = 5555;//5280;//5555;
        public string ComponentSecret = "28756a7ff5dce";
    }
}
