namespace ClusterSilo
{
    public class SiloConfigDefinition : n3q.Common.ConfigBag
    {
        public string ClusterId = "dev";
        public bool LocalhostClustering = true;
        public string ItemStateAzureTableConnectionString = "UseDevelopmentStorage=true";
        public string GrainStateAzureTableConnectionString = "UseDevelopmentStorage=true";
        public string ClusteringAzureTableConnectionString = "UseDevelopmentStorage=true";
        public string PubsubStoreAzureTableConnectionString = "UseDevelopmentStorage=true";
    }
}
