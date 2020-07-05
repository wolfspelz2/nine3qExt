using System.Collections.Generic;

namespace n3q.WebIt
{
    public class WebItConfig : WebItConfigDefinition
    {
        public void Load()
        {
            ConfigFile = CurrentFile;
            ConfigSequence += nameof(WebItConfig);

            if (Build == BuildConfiguration.Debug) {
                ClusterId = "dev";
                ClusteringAzureTableConnectionString = "DefaultEndpointsProtocol=https;AccountName=nine3qstoragetest;AccountKey=4Ov/kZAXYi4seMphX/t6jyTmvOuXVqf8P0M5QHd3b+mpHWJOzvo5gED9H23R4hMzxhMNueXoRyW4rk4BCctRuQ==;EndpointSuffix=core.windows.net";
                AdminTokens = new List<string> { "Token" };
                ItemServiceXmppUrl = "xmpp:itemsxmpp.dev.sui.li";
                ItemWebBaseUrl = "http://localhost:5000/";
            } else {
                ClusterId = "prod";
                LocalhostClustering = false;
                AdminTokens = new List<string>();
                ItemServiceXmppUrl = "xmpp:itemsxmpp.k8s.sui.li";
                ItemWebBaseUrl = "https://webit.k8s.sui.li/";
            }

            UnavailableUrl = ItemWebBaseUrl + "Embedded/Account?id={id}";
            ItemAppearanceBaseUrl = ItemWebBaseUrl + "images/Items/";
            ItemServiceWebApiUrl = ItemWebBaseUrl + "rpc";

            AdditionalConfigRoot = System.Environment.GetEnvironmentVariable(ConfigRootEnvironmentVariableName) ?? AdditionalConfigRoot;
            if (!string.IsNullOrEmpty(AdditionalConfigRoot)) {
                BaseFolder = AdditionalConfigRoot;
                Include(ConfigFile);
            }
        }
    }
}