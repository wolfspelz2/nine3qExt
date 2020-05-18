using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace nine3q.Items
{
    public class ItemSummaryRecorder : IInventoryChanges
    {
        public ItemIdSet AddedItems { get; } = new ItemIdSet();
        public ItemIdSet ChangedItems { get; } = new ItemIdSet();
        public ItemIdSet DeletedItems { get; } = new ItemIdSet();

        public List<string> NewTemplates { get; } = new List<string>();

        public bool IsChanged()
        {
            return (AddedItems.Count + ChangedItems.Count + DeletedItems.Count) > 0; ;
        }

        public ItemSummaryRecorder(Inventory inv)
        {
            Contract.Requires(inv != null);
            foreach (var change in inv.Changes) {
                switch (change.What) {
                    case ItemChange.Variant.CreateItem:
                        AddedItems.Add(change.ItemId);
                        ChangedItems.Add(change.ItemId);
                        DeletedItems.Remove(change.ItemId);
                        break;
                    case ItemChange.Variant.TouchItem:
                        ChangedItems.Add(change.ItemId);
                        DeletedItems.Remove(change.ItemId);
                        break;
                    case ItemChange.Variant.DeleteItem:
                        AddedItems.Remove(change.ItemId);
                        ChangedItems.Remove(change.ItemId);
                        DeletedItems.Add(change.ItemId);
                        break;
                    case ItemChange.Variant.AddProperty:
                    case ItemChange.Variant.SetProperty: {
                        var prop = Property.Get(change.Pid);
                        if (prop.Group == Property.Group.Operation) { break; }
                        if (Property.AreEquivalent(prop.Type, change.PreviousValue, change.Value)) { break; }
                        ChangedItems.Add(change.ItemId);
                        if (change.Pid == Pid.TemplateName) {
                            NewTemplates.Add(change.Value as string);
                        }
                    }
                    break;
                    case ItemChange.Variant.DeleteProperty:
                    case ItemChange.Variant.AddItemToCollection:
                    case ItemChange.Variant.RemoveItemFromCollection: {
                        var prop = Property.Get(change.Pid);
                        if (prop.Group == Property.Group.Operation) { break; }
                        ChangedItems.Add(change.ItemId);
                    }
                    break;
                }
            }

            foreach (var deletedId in DeletedItems) {
                if (AddedItems.Contains(deletedId)) {
                    AddedItems.Remove(deletedId);
                }
                if (ChangedItems.Contains(deletedId)) {
                    ChangedItems.Remove(deletedId);
                }
            }
        }

    }
}
