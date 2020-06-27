namespace n3q.Xmpp
{
    class XmppConfigProduction : XmppConfig
    {
        public void Load()
        {
            ConfigSequence += " XmppConfigProduction";
            ComponentSecret = "28756a7ff5dce";// "Jn3Gd9R5r6hgFGhu5drvU1bh"; //hw TODO change
        }
    }
}