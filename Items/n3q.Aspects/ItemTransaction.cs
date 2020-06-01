using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using n3q.Tools;

namespace n3q.Aspects
{
    public class ItemTransaction
    {
        public Guid Id = Guid.NewGuid();
        public static Guid WithoutTransaction = Guid.Empty;

        readonly List<ItemStub> _items = new List<ItemStub>();

        public async Task Begin(ItemStub item)
        {
            await AddItem(item);
        }

        public async Task AddItem(ItemStub item)
        {
            if (!_items.Contains(item)) {
                _items.Add(item);
                try {
                    await item.BeginTransaction();
                } catch (Exception ex) {
                    _ = ex;
                }
            }
        }

        public async Task Commit()
        {
            foreach (var item in _items) {
                await item.EndTransaction(true);
                item.Transaction = null;
            }
        }

        public async Task Cancel()
        {
            foreach (var item in _items) {
                await item.EndTransaction(false);
                item.Transaction = null;
            }
        }
    }
}
