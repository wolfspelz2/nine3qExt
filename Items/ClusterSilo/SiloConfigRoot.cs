namespace ClusterSilo
{
    class SiloConfigRoot : SiloConfig
    {
        public void Load()
        {
            ConfigSequence += "SiloConfigRoot";
            Mode = RunMode.ToString();
            if (RunMode == RunModes.Development) {

                if (DevelopmentRemoteConfig) {
                    Include("https://raw.githubusercontent.com/wolfspelz/EQtldeHSgvqFPObzet/master/SiloConfigDevelopment.cs?token=AATIDC3PG4LTMCO46V3YWAK67X4PA");
                } else {
                    GrainStateAzureTableConnectionString = "DefaultEndpointsProtocol=https;AccountName=nine3qstoragetest;AccountKey=4Ov/kZAXYi4seMphX/t6jyTmvOuXVqf8P0M5QHd3b+mpHWJOzvo5gED9H23R4hMzxhMNueXoRyW4rk4BCctRuQ==;EndpointSuffix=core.windows.net";
                }

            } else {

                Include("https://raw.githubusercontent.com/wolfspelz/EQtldeHSgvqFPObzet/master/SiloConfigProduction.cs?token=AATIDCZS4BU6T7MEF6XMOBS67X4Q2");

            }
        }
    }
}
