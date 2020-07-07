using System;
using System.Threading.Tasks;

namespace n3q.Aspects
{
    public class VoidTransaction : ITransaction
    {
        public Guid Id => ItemTransaction.WithoutTransaction;

        public Task AddItem(ItemStub item)
        {
            return Task.CompletedTask;
        }

        public Task Begin(ItemStub item)
        {
            return Task.CompletedTask;
        }

        public Task Cancel()
        {
            return Task.CompletedTask;
        }

        public Task Commit()
        {
            return Task.CompletedTask;
        }
    }
}