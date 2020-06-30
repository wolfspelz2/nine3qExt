﻿using System.Collections.Generic;

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
                ClusteringAzureTableConnectionString = "DefaultEndpointsProtocol=https;AccountName=nine3qstoragetest;AccountKey=4Ov/kZAXYi4seMphX/t6jyTmvOuXVqf8P0M5QHd3b+mpHWJOzvo5gED9H23R4hMzxhMNueXoRyW4rk4BCctRuQ==;EndpointSuffix=core.windows.net";

                AdminTokens = new List<string> { "Token" };
                ItemServiceXmppUrl = "xmpp:itemsxmpp.dev.sui.li";

                BaseUrl = "http://localhost:5000/";

            } else {

                ClusterId = "prod";
                LocalhostClustering = false;

                AdminTokens = new List<string>();
                ItemServiceXmppUrl = "xmpp:itemsxmpp.k8s.sui.li";

                BaseUrl = "https://n3qweb.k8s.sui.li/";

            }

            UnavailableUrl = BaseUrl + "Embedded/Account?id={id}";
            ItemBaseUrl = BaseUrl + "images/Items/";

            AdditionalBaseFolder = System.Environment.GetEnvironmentVariable("N3Q_CONFIG_ROOT") ?? AdditionalBaseFolder;
            if (!string.IsNullOrEmpty(AdditionalBaseFolder)) {
                BaseFolder = AdditionalBaseFolder;
                Include(ConfigFile);
            }
        }
    }
}