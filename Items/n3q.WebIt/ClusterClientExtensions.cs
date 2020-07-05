using Orleans;
using n3q.Aspects;
using n3q.Tools;

namespace n3q.WebIt
{
    public static class ClusterClientExtensions
    {
        public static ItemStub GetItemStub(this IClusterClient self, string itemId)
        {
            var itemClient = new OrleansClusterClient(self, itemId);
            var itemStub = new ItemStub(itemClient);
            return itemStub;
        }
    }
}
