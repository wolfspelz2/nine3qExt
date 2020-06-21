namespace LocalSilo
{
    public class LocalSiloConfig : ConfigSharp.Container
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

        public string Mode = "_empty_";
        public string GrainStateAzureTableConnectionString = "UseDevelopmentStorage=true";
    }
}
