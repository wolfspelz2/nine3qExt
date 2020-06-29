namespace ClusterSilo
{
    public class SiloConfigProduction : SiloConfigDefinition
    {
        public void Load()
        {
            ConfigSequence += " " + nameof(SiloConfigProduction);

            //hw TODO recreate and change all
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=nine3qstorage;AccountKey=HnJJaHTKXvgvGbmQGe6ptVeyz7TIJY5E1EDabtxq5KCmzrxmiz66YpiK7Zj9HdnNuqRHxoWXG8WDCjIfM/7wQg==;EndpointSuffix=core.windows.net";
            ItemStateAzureTableConnectionString = connectionString;
            GrainStateAzureTableConnectionString = connectionString;
            ClusteringAzureTableConnectionString = connectionString;
            PubsubStoreAzureTableConnectionString = connectionString;

        }
    }
}
