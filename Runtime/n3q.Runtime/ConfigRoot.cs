using ConfigSharp;

namespace n3q.Runtime
{
    class ConfigRoot : SharpConfigurationBag
    {
        public void Load()
        {
            Data["GrainBName"] = "b";
        }
    }
}