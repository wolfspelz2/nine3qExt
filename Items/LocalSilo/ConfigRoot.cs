namespace LocalSilo
{
    class ConfigRoot : LocalSiloConfig
    {
        public void Load()
        {
            ConfigSequence += "ConfigRoot ";
            Mode = RunMode.ToString();
            if (RunMode == RunModes.Production) {
                GrainStateAzureTableConnectionString = "";
            }
        }
    }
}
