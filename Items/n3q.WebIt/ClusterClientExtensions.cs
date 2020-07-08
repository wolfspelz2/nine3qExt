using Orleans;
using n3q.Aspects;
using n3q.Tools;

namespace n3q.WebIt
{
    public static class ClusterClientExtensions
    {
        public static ItemWriter GetItemStub(this IClusterClient self, string itemId)
        {
            var itemClient = new OrleansClusterItemClient(self, itemId);
            var itemStub = new ItemWriter(itemClient);
            return itemStub;
        }
    }
}
