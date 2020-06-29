namespace n3q.Common
{
    public class ConfigBag: ConfigSharp.ConfigBag
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
        public string AdditionalBaseFolder = null;
    }
}
