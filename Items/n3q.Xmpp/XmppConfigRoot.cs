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

                if (DevelopmentRemoteConfig) {
                    Include("https://raw.githubusercontent.com/wolfspelz/EQtldeHSgvqFPObzet/master/XmppConfigDevelopment.cs?token=AATIDC2BSYEPXIUN5LZZTES67X4VC");
                } else {
                ComponentSecret = "28756a7ff5dce";
                }

            } else {

                ComponentHost = "itemsxmpp.weblin.com";
                ComponentDomain = "itemsxmpp.weblin.com";
                ComponentPort = 5280;

                Include("https://raw.githubusercontent.com/wolfspelz/EQtldeHSgvqFPObzet/master/XmppConfigProduction.cs?token=AATIDC5Y5XYZVRJXDQUPY4S67X4WQ");

            }
        }
    }
}