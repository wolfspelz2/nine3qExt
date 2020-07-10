using Orleans;
using n3q.GrainInterfaces;
using System.Threading.Tasks;

namespace n3q.Aspects
{
       public abstract class ItemClusterClientBase: IItemClusterClient
    {
        public abstract IItemClient GetItemClient(string itemId);

        public ItemWriter GetItemWriter(string itemId)
        {
            var itemClient = GetItemClient(itemId);
            return new ItemWriter(itemClient, new ItemTransaction());
        }

        public ItemReader GetItemReader(string itemId)
        {
            var itemClient = GetItemClient(itemId);
            return new ItemReader(itemClient, new VoidTransaction());
        }

        public async Task Transaction(string itemId, ItemStub.TransactionWrappedCode transactedCode)
        {
            var item = GetItemWriter(itemId);
            await item.WithTransaction(transactedCode);
        }
    }
}
