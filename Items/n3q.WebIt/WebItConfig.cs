using System.Collections.Generic;

namespace n3q.WebIt
{
    public class WebItConfig : WebItConfigDefinition
    {
        public void Load()
        {
            ConfigFile = CurrentFile;
            ConfigSequence += nameof(WebItConfig);

            AdditionalConfigRoot = System.Environment.GetEnvironmentVariable(ConfigRootEnvironmentVariableName) ?? AdditionalConfigRoot;
            if (!string.IsNullOrEmpty(AdditionalConfigRoot)) {
                BaseFolder = AdditionalConfigRoot;
                Include(SetupFile);
            }

            if (Setup == SetupMode.Development) {
                ClusterId = "dev";
                ClusteringAzureTableConnectionString = "DefaultEndpointsProtocol=https;AccountName=nine3qstoragetest;AccountKey=4Ov/kZAXYi4seMphX/t6jyTmvOuXVqf8P0M5QHd3b+mpHWJOzvo5gED9H23R4hMzxhMNueXoRyW4rk4BCctRuQ==;EndpointSuffix=core.windows.net";
                AdminTokens = new List<string> { "Token" };
                ItemServiceXmppUrl = "xmpp:itemsxmpp.dev.sui.li";
                ItemWebBaseUrl = "http://localhost:5000/";
            } else if (Setup == SetupMode.Stage) {
                ClusterId = "stage01";
                LocalhostClustering = false;
                AdminTokens = new List<string>();
                ItemServiceXmppUrl = "xmpp:itemsxmpp.vulcan.weblin.com";
                ItemWebBaseUrl = "https://stage01-webit.vulcan.weblin.com/";
            } else {
                ClusterId = "prod";
                LocalhostClustering = false;
                AdminTokens = new List<string>();
                ItemServiceXmppUrl = "xmpp:itemsxmpp.vulcan.weblin.com";
                ItemWebBaseUrl = "https://webit.vulcan.weblin.com/";
            }

            UnavailableUrl = ItemWebBaseUrl + "Embedded/Account?id={id}";
            ItemAppearanceBaseUrl = ItemWebBaseUrl + "images/Items/";
            ItemIframeBaseUrl = ItemWebBaseUrl + "ItemFrame/";
            ItemServiceWebApiUrl = ItemWebBaseUrl + "rpc";

            AdditionalConfigRoot = System.Environment.GetEnvironmentVariable(ConfigRootEnvironmentVariableName) ?? AdditionalConfigRoot;
            if (!string.IsNullOrEmpty(AdditionalConfigRoot)) {
                BaseFolder = AdditionalConfigRoot;
                Include(ConfigFile);
            }
        }
    }
}