using ConfigSharp;

namespace n3q.Xmpp
{
    class ConfigRoot : SharpConfigurationBag
    {
        public void Load()
        {
            Data["a"] = "b";
        }
    }
}