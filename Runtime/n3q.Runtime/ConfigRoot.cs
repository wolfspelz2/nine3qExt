using ConfigSharp;

namespace n3q.Runtime
{
    class ConfigRoot : RuntimeConfig
    {
        public void Load()
        {
            Dummy = "x";
        }
    }
}