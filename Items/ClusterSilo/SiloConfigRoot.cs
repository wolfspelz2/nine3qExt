﻿namespace ClusterSilo
{
    class SiloConfigRoot : SiloConfig
    {
        public void Load()
        {
            ConfigSequence += "SiloConfigRoot";
            Mode = RunMode.ToString();
            if (RunMode == RunModes.Development) {

                GrainStateAzureTableConnectionString = "DefaultEndpointsProtocol=https;AccountName=nine3qstoragetest;AccountKey=4Ov/kZAXYi4seMphX/t6jyTmvOuXVqf8P0M5QHd3b+mpHWJOzvo5gED9H23R4hMzxhMNueXoRyW4rk4BCctRuQ==;EndpointSuffix=core.windows.net";

            } else {

                Include("SiloConfigProduction.cs");

            }
        }
    }
}
