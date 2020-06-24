using ConfigSharp;

namespace n3q.Web
{
    public class  WebConfig: SharpConfigurationBag
    {
        public string AdminTokens;
        public string ItemServiceXmppUrl;
        public string WebBaseUrl;
    }
}