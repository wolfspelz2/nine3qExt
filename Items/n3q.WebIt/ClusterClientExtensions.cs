using System.Threading.Tasks;
using Orleans;
using n3q.Aspects;

namespace n3q.WebIt
{
    public static class ClusterClientExtensions
    {
        public static ItemWriter GetItemWriter(this IClusterClient self, string itemId)
        {
            var itemClient = new OrleansClusterItemClient(self, itemId);
            var itemStub = new ItemWriter(itemClient);
            return itemStub;
        }

        public static async Task Transaction(this IClusterClient self, string itemId, ItemStub.TransactionWrappedCode transactedCode)
        {
            var item = self.GetItemWriter(itemId);
            await item.WithTransaction(transactedCode);
        }
    }
}
