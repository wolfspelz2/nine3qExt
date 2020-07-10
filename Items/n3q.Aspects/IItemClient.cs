using System.Threading.Tasks;
using n3q.GrainInterfaces;

namespace n3q.Aspects
{
    public interface IItemClient
    {
        string GetId();
        IItem GetItem();
        IItemClient CloneFor(string otherId);
    }

    public interface IItemClusterClient
    {
        IItemClient GetItemClient(string itemId);
        ItemWriter GetItemWriter(string itemId);
        ItemReader GetItemReader(string itemId);
        Task Transaction(string itemId, ItemStub.TransactionWrappedCode transactedCode);
    }

    //public static class OrleansItemClusterClientExtensions
    //{
    //    public static async Task Transaction(this OrleansItemClusterClient self, string itemId, ItemStub.TransactionWrappedCode transactedCode)
    //    {
    //        var itemClient = new OrleansClusterItemClient(self, itemId);
    //        var itemStub = new ItemWriter(itemClient);
    //        await itemStub.WithTransaction(transactedCode);
    //    }
    //}
}
