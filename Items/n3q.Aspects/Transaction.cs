﻿using System;
using System.Collections.Generic;
using System.Text;
using Orleans;
using n3q.Tools;

namespace n3q.Aspects
{
    public class Transaction
    {
        public Guid Id = Guid.NewGuid();

        readonly HashSet<ItemStub> _items = new HashSet<ItemStub>();

        public Transaction()
        {
        }

        internal void AddItem(ItemStub item)
        {
            if (!_items.Contains(item)) {
                _items.Add(item);
            }
        }

        public void Commit()
        {
            foreach (var item in _items) {
                item.EndTransaction(Id, true).PerformAsyncTaskWithoutAwait(t => {
                    // Log?
                });
            }
        }

        public void Cancel()
        {
            foreach (var item in _items) {
                item.EndTransaction(Id, false).PerformAsyncTaskWithoutAwait(t => {
                    // Log?
                });
            }
        }

    }
}
