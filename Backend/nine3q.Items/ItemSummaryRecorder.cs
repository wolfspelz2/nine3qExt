using System.Collections.Generic;

namespace nine3q.Items
{
    public class ItemSummaryRecorder : IInventoryChanges
    {
        public longSet AddedItems = new longSet();
        public longSet ChangedItems = new longSet();
        public longSet DeletedItems = new longSet();

        public List<string> NewTemplates = new List<string>();

        public bool IsChanged()
        {
            return (AddedItems.Count + ChangedItems.Count + DeletedItems.Count) > 0; ;
        }

        public ItemSummaryRecorder(Inventory inv)
        {
            foreach (var change in inv.Changes) {
                switch (change.What) {
                    case ItemChange.Variant.CreateItem:
                        AddedItems.Add(change.long);
                        ChangedItems.Add(change.long);
                        DeletedItems.Remove(change.long);
                        break;
                    case ItemChange.Variant.TouchItem:
                        ChangedItems.Add(change.long);
                        DeletedItems.Remove(change.long);
                        break;
                    case ItemChange.Variant.DeleteItem:
                        AddedItems.Remove(change.long);
                        ChangedItems.Remove(change.long);
                        DeletedItems.Add(change.long);
                        break;
                    case ItemChange.Variant.AddProperty:
                    case ItemChange.Variant.SetProperty: {
                        var prop = Property.Get(change.Pid);
                        if (prop.Group == Property.Group.Operation) { break; }
                        if (Property.AreEquivalent(prop.Type, change.PreviousValue, change.Value)) { break; }
                        ChangedItems.Add(change.long);
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
                        ChangedItems.Add(change.long);
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
