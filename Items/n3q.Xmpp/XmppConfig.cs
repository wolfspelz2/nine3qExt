namespace n3q.Xmpp
{
    class XmppConfig : XmppConfigDefinition
    {
        public void Load()
        {
            ConfigFile = CurrentFile;
            ConfigSequence += nameof(XmppConfig);

            if (Build == BuildConfiguration.Debug) {

                ClusterId = "dev";
                ClusteringAzureTableConnectionString = "DefaultEndpointsProtocol=https;AccountName=nine3qstoragetest;AccountKey=4Ov/kZAXYi4seMphX/t6jyTmvOuXVqf8P0M5QHd3b+mpHWJOzvo5gED9H23R4hMzxhMNueXoRyW4rk4BCctRuQ==;EndpointSuffix=core.windows.net";

                ComponentHost = "xmpp.dev.sui.li";
                ComponentPort = 5555;//5347;//5555;
                ComponentDomain = "itemsxmpp.dev.sui.li";
                ComponentSecret = "28756a7ff5dce";

            } else {

                //hw TODO recreate and change all
                ClusterId = "prod";
                LocalhostClustering = false;

                ComponentHost = "prosody-xmpp.n3q-prod.svc.cluster.local";
                ComponentPort = 5347;
                ComponentDomain = "itemsxmpp.k8s.sui.li";

            }

            AdditionalConfigRoot = System.Environment.GetEnvironmentVariable("N3Q_CONFIG_ROOT") ?? AdditionalConfigRoot;
            if (!string.IsNullOrEmpty(AdditionalConfigRoot)) {
                BaseFolder = AdditionalConfigRoot;
                Include(ConfigFile);
            }
        }
    }
}