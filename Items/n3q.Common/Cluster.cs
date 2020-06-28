using System;

namespace n3q.Common
{
    public static class Cluster
    {
        public const string ServiceId = "WeblinItems";
        public const string SimpleMessageStreamProviderName = "SMSProvider";
        public const string MemoryGrainStorageProviderName = "PubSubStore";
        public const int LengthOfItemIdPrefixFromTemplate = 3;
        public const string JsonFileStorageRoot = @"C:\Heiner\github-nine3q\Items\Test\JsonFileStorage";
        public const string KeyValueFileStorageRoot = @"C:\Heiner\github-nine3q\Items\Test\KeyValueFileStorage";
        //public const string JsonFileStorageRoot = @"./GrainJsonFileStorage";
    }
}
