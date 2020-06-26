namespace LocalSilo
{
    public class LocalSiloConfig : ConfigSharp.ConfigBag
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
        public string ConfigFile = "ConfigRoot.cs";
        public string Mode = "_empty_";
        public string GrainStateAzureTableConnectionString = "UseDevelopmentStorage=true";
    }
}
