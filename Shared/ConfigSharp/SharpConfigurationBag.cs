using System.Collections.Generic;

namespace ConfigSharp
{
    public class SharpConfigurationBag : ConfigBag
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

        public Dictionary<string, string> Data = new Dictionary<string, string>();

        public SharpConfigurationBag()
        {
            Data["RunMode"] = RunMode.ToString();
        }
    }
}
