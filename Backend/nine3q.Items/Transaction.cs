using System;
using System.Collections.Generic;
using nine3q.Lib;

namespace nine3q.Items
{
    public class Transaction : IDisposable
    {
        string _name = "";
        public string Name { get { return _name; } }

        bool _canceled = false;
        Inventory _inventory { get; set; }
        List<ItemChange> _changes = new List<ItemChange>();

        public Transaction(Inventory inv, string name = null)
        {
            _inventory = inv;
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
            _inventory._CancelTransaction(this);
        }

        public void Commit()
        {
            _inventory._CommitTransaction(this);
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
