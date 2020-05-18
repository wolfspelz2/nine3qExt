using System;
using System.Collections.Generic;
using nine3q.Tools;

namespace nine3q.Items
{
    public sealed class InventoryTransaction : IDisposable
    {
        readonly string _name = "";
        public string Name { get { return _name; } }

        bool _canceled = false;
        Inventory Inventory { get; set; }
        List<ItemChange> _changes = new List<ItemChange>();

        public InventoryTransaction(Inventory inv, string name = null)
        {
            Inventory = inv;
            _name = name ?? Utils.GetMethodName(2, fullName: true);
        }

        internal void AddChange(ItemChange change)
        {
            bool skipChange = false;

            if (!skipChange) {
                _changes.Add(change);
            }
        }

        public void Cancel()
        {
            _canceled = true;
            Inventory.CancelTransaction(this);
        }

        public void Commit()
        {
            Inventory.CommitTransaction(this);
        }

        internal List<ItemChange> GetChanges()
        {
            return _changes;
        }

        internal void ResetChanges()
        {
            _changes = null;
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            if (!_canceled) {
                Commit();
            }
        }

        #endregion
    }
}
