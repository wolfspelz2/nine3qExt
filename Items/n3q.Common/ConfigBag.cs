namespace n3q.Common
{
    public class ConfigBag : ConfigSharp.ConfigBag
    {
        public enum BuildConfiguration
        {
            Debug,
            Release
        }

        public BuildConfiguration Build =
#if DEBUG
            BuildConfiguration.Debug;
#else
            BuildConfiguration.Release;
#endif

        public string ConfigSequence = "";
        public string ConfigFile = "Config.cs";
        public string ConfigRootEnvironmentVariableName = "N3Q_CONFIG_ROOT";
        public string AdditionalConfigRoot = null;
    }
}
