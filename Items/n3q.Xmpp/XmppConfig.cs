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
                ExtensionWebBaseUrl = "http://localhost:5001/";
            } else {
                ClusterId = "prod";
                LocalhostClustering = false;
                ComponentHost = "prosody-xmpp.n3q-prod.svc.cluster.local";
                ComponentPort = 5347;
                ComponentDomain = "itemsxmpp.k8s.sui.li";
                ExtensionWebBaseUrl = "https://webex.k8s.sui.li/";
            }

            IdentificatorUrlTemplate = ExtensionWebBaseUrl + "Identity/Generated?avatarUrl={avatarUrl}&nickname={nickname}&digest={digest}&imageUrl={imageUrl}";

            AdditionalConfigRoot = System.Environment.GetEnvironmentVariable(ConfigRootEnvironmentVariableName) ?? AdditionalConfigRoot;
            if (!string.IsNullOrEmpty(AdditionalConfigRoot)) {
                BaseFolder = AdditionalConfigRoot;
                Include(ConfigFile);
            }
        }
    }
}