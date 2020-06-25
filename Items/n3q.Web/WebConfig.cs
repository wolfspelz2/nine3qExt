using System.Collections.Generic;
using ConfigSharp;

namespace n3q.Web
{
    public class WebConfig : ConfigBag
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

        public bool UseIntegratedCluster = false;
        public List<string> AdminTokens = new List<string> { };
        public string ItemServiceXmppUrl = "xmpp:itemsxmpp.weblin.com";
        public string WebBaseUrl;
    }
}