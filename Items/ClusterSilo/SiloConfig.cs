namespace ClusterSilo
{
    class SiloConfig : SiloConfigDefinition
    {
        public void Load()
        {
            ConfigFile = CurrentFile;
            ConfigSequence += nameof(SiloConfig);

            AdditionalConfigRoot = System.Environment.GetEnvironmentVariable(ConfigRootEnvironmentVariableName) ?? AdditionalConfigRoot;
            if (!string.IsNullOrEmpty(AdditionalConfigRoot)) {
                BaseFolder = AdditionalConfigRoot;
                Include(SetupFile);
            }

            if (Setup == SetupMode.Development) {

                ClusterId = "dev";

                var connectionString = n3q.Common.Cluster.DevelopmentAzureTableConnectionString;
                if (!DevelopmentSimulatorStorage) {
                    connectionString = "DefaultEndpointsProtocol=https;AccountName=nine3qstoragetest;AccountKey=4Ov/kZAXYi4seMphX/t6jyTmvOuXVqf8P0M5QHd3b+mpHWJOzvo5gED9H23R4hMzxhMNueXoRyW4rk4BCctRuQ==;EndpointSuffix=core.windows.net";
                }
                ItemStateAzureTableConnectionString = connectionString;
                GrainStateAzureTableConnectionString = connectionString;
                ClusteringAzureTableConnectionString = connectionString;
                PubsubStoreAzureTableConnectionString = connectionString;

            } else if (Setup == SetupMode.Stage) {
                ClusterId = "stage01";
                LocalhostClustering = false;
            } else {
                ClusterId = "prod";
                LocalhostClustering = false;
            }

            AdditionalConfigRoot = System.Environment.GetEnvironmentVariable(ConfigRootEnvironmentVariableName) ?? AdditionalConfigRoot;
            if (!string.IsNullOrEmpty(AdditionalConfigRoot)) {
                BaseFolder = AdditionalConfigRoot;
                Include(ConfigFile);
            }
        }
    }
}
