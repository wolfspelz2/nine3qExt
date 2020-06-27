namespace LocalSilo
{
    class ConfigRoot : LocalSiloConfig
    {
        public void Load()
        {
            ConfigSequence += nameof(ConfigRoot);
            Mode = RunMode.ToString();
            if (RunMode == RunModes.Production) {
                GrainStateAzureTableConnectionString = "";
            }
        }
    }
}
