namespace ClusterSilo
{
    public class SiloConfigDefinition : n3q.Common.ConfigBag
    {
        public string ClusterId = n3q.Common.Cluster.DevelopmentClusterId;
        public bool LocalhostClustering = n3q.Common.Cluster.DevelopmentLocalhostClustering;
        public bool DevelopmentSimulatorStorage = n3q.Common.Cluster.DevelopmentAzureSimulatorStorage;
        public string ItemStateAzureTableConnectionString = n3q.Common.Cluster.DevelopmentAzureTableConnectionString;
        public string GrainStateAzureTableConnectionString = n3q.Common.Cluster.DevelopmentAzureTableConnectionString;
        public string ClusteringAzureTableConnectionString = n3q.Common.Cluster.DevelopmentAzureTableConnectionString;
        public string PubsubStoreAzureTableConnectionString = n3q.Common.Cluster.DevelopmentAzureTableConnectionString;
    }
}
