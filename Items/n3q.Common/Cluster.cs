using System;

namespace n3q.Common
{
    public static class Cluster
    {
        public const string ServiceId = "WeblinItems";
        public const string SimpleMessageStreamProviderName = "SMSProvider";
        public const string MemoryGrainStorageProviderName = "PubSubStore";
        public const int LengthOfItemIdPrefixFromTemplate = 3;
        public static bool DevelopmentLocalhostClustering = true;
        public const string DevelopmentClusterId = "dev";
        public const string DevelopmentAzureTableConnectionString = "UseDevelopmentStorage=true";
    }
}
