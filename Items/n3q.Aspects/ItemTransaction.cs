using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using n3q.Tools;

namespace n3q.Aspects
{
    public class ItemTransaction
    {
        public Guid Id = Guid.NewGuid();

        readonly HashSet<ItemStub> _items = new HashSet<ItemStub>();

        public async Task Begin(ItemStub item)
        {
            await AddItem(item);
        }

        public async Task AddItem(ItemStub item)
        {
            if (!_items.Contains(item)) {
                _items.Add(item);
                await item.BeginTransaction(Id);
            }
        }

        public async Task Commit()
        {
            foreach (var item in _items) {
                await item.EndTransaction(Id, true);
            }
        }

        public async Task Cancel()
        {
            foreach (var item in _items) {
                await item.EndTransaction(Id, false);
            }
        }
    }
}
