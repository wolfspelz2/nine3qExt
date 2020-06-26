using System;

namespace n3q.Common
{
    public static class ItemService
    {
        public const string StreamProvider = Cluster.SimpleMessageStreamProviderName;
        public const string StreamNamespace = "Default";
        public static Guid StreamGuid = Guid.Parse("{6129CB50-60F7-45AA-98E0-FB50C4C24221}");
        public const string JsonFileStorageRoot = @"C:\Heiner\github-nine3q\Items\Test\JsonFileStorage";
        public const string KeyValueFileStorageRoot = @"C:\Heiner\github-nine3q\Items\Test\KeyValueFileStorage";
        //public const string JsonFileStorageRoot = @"./GrainJsonFileStorage";
    }
}
