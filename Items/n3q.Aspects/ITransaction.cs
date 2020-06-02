using System;
using System.Threading.Tasks;

namespace n3q.Aspects
{
    public interface ITransaction
    {
        Guid Id { get; }
        Task Begin(ItemStub item);
        Task AddItem(ItemStub item);
        Task Cancel();
        Task Commit();
    }
}