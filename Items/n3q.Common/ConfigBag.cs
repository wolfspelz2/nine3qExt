namespace n3q.Common
{
    public class ConfigBag : ConfigSharp.ConfigBag
    {
        public enum SetupMode
        {
            Development,
            Production,
            Stage,
        }

        public SetupMode Setup =
#if DEBUG
            SetupMode.Development;
#else
            SetupMode.Production;
#endif

        public string ConfigSequence = "";
        public string SetupFile = "Setup.cs";
        public string ConfigFile = "Config.cs";
        public string ConfigRootEnvironmentVariableName = "N3Q_CONFIG_ROOT";
        public string AdditionalConfigRoot = null;
    }
}
