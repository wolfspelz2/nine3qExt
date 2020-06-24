namespace n3q.Xmpp
{
    class ConfigRoot : XmppConfig
    {
        public void Load()
        {
            ConfigSequence += "ConfigRoot ";
            Mode = RunMode.ToString();

            if (RunMode == RunModes.Production) {
                ComponentHost = "itemsxmpp.weblin.com";
                ComponentDomain = "itemsxmpp.weblin.com";
                ComponentPort = 5280;
                ComponentSecret = "Jn3Gd9R5r6hgFGhu5drvU1bh";
            } else {
                ComponentHost = "itemsxmpp.dev.sui.li";
                ComponentDomain = "itemsxmpp.dev.sui.li";
                ComponentPort = 5555;//5280;//5555;
                ComponentSecret = "28756a7ff5dce";
            }
        }
    }
}