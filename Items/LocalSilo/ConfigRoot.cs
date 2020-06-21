namespace LocalSilo
{
    class ConfigRoot : LocalSilo.LocalSiloConfig
    {
        public void Load()
        {
            Mode = RunMode.ToString();
            if (RunMode == RunModes.Production) {
                GrainStateAzureTableConnectionString = "";
            }
        }
    }
}
