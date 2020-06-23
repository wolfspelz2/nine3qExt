using ConfigSharp;

namespace n3q.Xmpp
{
    class ConfigRoot : SharpConfigurationBag
    {
        public void Load()
        {
            Data["GrainBName"] = "b";

            if (RunMode == RunModes.Development) {
                Data["AdminTokens"] = "Token";
            }
        }
    }
}