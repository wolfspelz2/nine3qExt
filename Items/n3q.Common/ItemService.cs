using System;

namespace n3q.Common
{
    public static class ItemService
    {
        public const string StreamProvider = Cluster.SimpleMessageStreamProviderName;
        public const string StreamNamespace = "Default";
        public static Guid StreamGuid = Guid.Parse("{6129CB50-60F7-45AA-98E0-FB50C4C24221}");
        public const string ItemBaseVar = "{image.item.nine3q}";
        public const string ItemIframeVar = "{iframe.item.nine3q}";
        public const string WebItConfigItemId = "config-RwUgdyMeJDk49QJPOPvxjAxoiy6x56t2aq4vYFG";
    }
}
