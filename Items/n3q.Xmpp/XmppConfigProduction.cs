namespace n3q.Xmpp
{
    class XmppConfigProduction : XmppConfigDefinition
    {
        public void Load()
        {
            ConfigSequence += " " + nameof(XmppConfigProduction);

            //hw TODO change
            ClusteringAzureTableConnectionString = "DefaultEndpointsProtocol=https;AccountName=nine3qstorage;AccountKey=HnJJaHTKXvgvGbmQGe6ptVeyz7TIJY5E1EDabtxq5KCmzrxmiz66YpiK7Zj9HdnNuqRHxoWXG8WDCjIfM/7wQg==;EndpointSuffix=core.windows.net";

            //hw TODO change
            ComponentSecret = "28756a7ff5dce";// "Jn3Gd9R5r6hgFGhu5drvU1bh"; 
        }
    }
}