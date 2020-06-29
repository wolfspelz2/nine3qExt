using System.Collections.Generic;

namespace n3q.Web
{
    public class WebConfig : WebConfigDefinition
    {
        public void Load()
        {
            ConfigFile = CurrentFile;
            ConfigSequence += nameof(WebConfig);

            if (Build == BuildConfiguration.Debug) {

                ClusterId = "dev";
                LocalhostClustering = false;
                ClusteringAzureTableConnectionString = "DefaultEndpointsProtocol=https;AccountName=nine3qstoragetest;AccountKey=4Ov/kZAXYi4seMphX/t6jyTmvOuXVqf8P0M5QHd3b+mpHWJOzvo5gED9H23R4hMzxhMNueXoRyW4rk4BCctRuQ==;EndpointSuffix=core.windows.net";

                AdminTokens = new List<string> { "Token" };
                ItemServiceXmppUrl = "xmpp:itemsxmpp.dev.sui.li";
                UnavailableUrl = "http://localhost:5000/Embedded/Account?id={id}";
                ItemBaseUrl = "http://localhost:5000/images/Items/";

            } else {

                ClusterId = "prod";
                LocalhostClustering = false;

                AdminTokens = new List<string>();
                ItemServiceXmppUrl = "xmpp:itemsxmpp.weblin.com";
                UnavailableUrl = "https://itemsweb.weblin.com/Embedded/Account?id={id}";
                ItemBaseUrl = "https://itemsweb.weblin.com/images/Items/";

            }

            AdditionalBaseFolder = System.Environment.GetEnvironmentVariable("N3Q_CONFIG_ROOT") ?? AdditionalBaseFolder;
            if (!string.IsNullOrEmpty(AdditionalBaseFolder)) {
                BaseFolder = AdditionalBaseFolder;
                Include(ConfigFile);
            }
        }
    }
}