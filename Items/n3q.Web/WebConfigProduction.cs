using System.Collections.Generic;

namespace n3q.Web
{
    public class WebConfigProduction : WebConfig
    {
        public void Load()
        {
            ConfigSequence += " " + nameof(WebConfigProduction);

            //hw TODO recreate and change all
            ClusteringAzureTableConnectionString = "DefaultEndpointsProtocol=https;AccountName=nine3qstorage;AccountKey=HnJJaHTKXvgvGbmQGe6ptVeyz7TIJY5E1EDabtxq5KCmzrxmiz66YpiK7Zj9HdnNuqRHxoWXG8WDCjIfM/7wQg==;EndpointSuffix=core.windows.net";

            //hw TODO change
            AdminTokens = new List<string> { "lgAkQAHJvxSm36ddWaMt" };
        }
    }
}