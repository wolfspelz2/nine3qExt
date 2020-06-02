using System;

namespace n3q.Common
{
    public static class Cluster
    {
        public const string DevClusterId = "dev";
        public const string TestClusterId = "test";
        public const string ServiceId = "WeblinItems";
        public const string SimpleMessageStreamProviderName = "SMSProvider";
        public const string MemoryGrainStorageProviderName = "PubSubStore";
        public const string MemoryGrainJsonFileStorageRoot = @"C:\Heiner\github-nine3q\Items\Test\MemoryGrainJsonFileStorage";
    }
}
