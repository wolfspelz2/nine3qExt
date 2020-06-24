using ConfigSharp;

namespace n3q.Runtime
{
    public class RuntimeConfig : ConfigBag
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

        public string Dummy;
    }
}
