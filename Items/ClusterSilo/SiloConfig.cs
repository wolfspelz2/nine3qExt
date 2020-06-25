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
        public bool DevelopmentRemoteConfig = true;
        public string Mode = "_empty_";
        public string GrainStateAzureTableConnectionString = "UseDevelopmentStorage=true";
    }
}
