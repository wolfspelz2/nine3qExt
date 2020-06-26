namespace n3q.Xmpp
{
    class XmppConfigRoot : XmppConfig
    {
        public void Load()
        {
            ConfigSequence += "XmppConfigRoot";
            Mode = RunMode.ToString();

            if (RunMode == RunModes.Development) {

                ComponentHost = "itemsxmpp.dev.sui.li";
                ComponentDomain = "itemsxmpp.dev.sui.li";
                ComponentPort = 5555;//5280;//5555;

                ComponentSecret = "28756a7ff5dce";

            } else {

                ComponentHost = "itemsxmpp.weblin.com";
                ComponentDomain = "itemsxmpp.weblin.com";
                ComponentPort = 5280;

                Include("XmppConfigProduction.cs");

            }
        }
    }
}