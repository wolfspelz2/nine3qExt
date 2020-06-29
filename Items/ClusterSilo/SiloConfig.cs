namespace ClusterSilo
{
    class SiloConfig : SiloConfigDefinition
    {
        public void Load()
        {
            ConfigFile = CurrentFile;
            ConfigSequence += nameof(SiloConfig);

            if (Build == BuildConfiguration.Debug) {

                ClusterId = "dev";
                LocalhostClustering = false;

                var connectionString = "DefaultEndpointsProtocol=https;AccountName=nine3qstoragetest;AccountKey=4Ov/kZAXYi4seMphX/t6jyTmvOuXVqf8P0M5QHd3b+mpHWJOzvo5gED9H23R4hMzxhMNueXoRyW4rk4BCctRuQ==;EndpointSuffix=core.windows.net";
                ItemStateAzureTableConnectionString = connectionString;
                GrainStateAzureTableConnectionString = connectionString;
                ClusteringAzureTableConnectionString = connectionString;
                PubsubStoreAzureTableConnectionString = connectionString;

            } else {

                ClusterId = "prod";
                LocalhostClustering = false;

            }

            AdditionalBaseFolder = System.Environment.GetEnvironmentVariable("N3Q_CONFIG_ROOT") ?? AdditionalBaseFolder;
            if (!string.IsNullOrEmpty(AdditionalBaseFolder)) {
                BaseFolder = AdditionalBaseFolder;
                Include(ConfigFile);
            }
        }
    }
}
