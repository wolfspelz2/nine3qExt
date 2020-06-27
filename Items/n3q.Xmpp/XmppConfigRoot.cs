﻿namespace n3q.Xmpp
{
    class XmppConfigRoot : XmppConfig
    {
        public void Load()
        {
            ConfigSequence += nameof(XmppConfigRoot);

            if (RunMode == RunModes.Development) {

                ClusterId = "dev";
                LocalhostClustering = false;
                ClusteringAzureTableConnectionString = "DefaultEndpointsProtocol=https;AccountName=nine3qstoragetest;AccountKey=4Ov/kZAXYi4seMphX/t6jyTmvOuXVqf8P0M5QHd3b+mpHWJOzvo5gED9H23R4hMzxhMNueXoRyW4rk4BCctRuQ==;EndpointSuffix=core.windows.net";

                ComponentHost = "itemsxmpp.dev.sui.li";
                ComponentDomain = "itemsxmpp.dev.sui.li";
                ComponentPort = 5555;//5280;//5555;
                ComponentSecret = "28756a7ff5dce";

            } else {

                //hw TODO recreate and change all
                ClusterId = "prod";
                LocalhostClustering = false;

                ComponentHost = "itemsxmpp.dev.sui.li";
                ComponentDomain = "itemsxmpp.dev.sui.li";
                ComponentPort = 5280;

                Include("XmppConfigProduction.cs");

            }
        }
    }
}