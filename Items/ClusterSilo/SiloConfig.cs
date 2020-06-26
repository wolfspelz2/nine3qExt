namespace ClusterSilo
{
    public class SiloConfig : ConfigSharp.ConfigBag
    {
        public enum RunModes
        {
            Development,
            Test,
            Staging,
            Production
        }

        public RunModes RunMode =
#if DEBUG
            RunModes.Development;
#else
            RunModes.Production;
#endif

        public string ConfigSequence = "";
        public string ConfigFile = "SiloConfigRoot.cs";

        public string ClusterId = "dev";
        public bool LocalhostClustering = true;
        public string ItemStateAzureTableConnectionString = "UseDevelopmentStorage=true";
        public string GrainStateAzureTableConnectionString = "UseDevelopmentStorage=true";
        public string ClusteringAzureTableConnectionString = "UseDevelopmentStorage=true";
        public string PubsubStoreAzureTableConnectionString = "UseDevelopmentStorage=true";
    }
}
