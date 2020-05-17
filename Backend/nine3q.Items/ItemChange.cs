namespace nine3q.Items
{
    public class ItemChange
    {
        public enum Variant
        {
            NoChange = 0,
            CreateItem,
            TouchItem,
            DeleteItem,
            AddProperty,
            SetProperty,
            DeleteProperty,
            AddItemToCollection,
            RemoveItemFromCollection,
        }

        public Variant What { get; set; }
        public long long { get; set; }
        public Item Item { get; set; } // Used by DeleteItem, to keep the last reference of undo
        public Pid Pid { get; set; }
        public object Value { get; set; }
        public object PreviousValue { get; set; }
        public long ChildId { get; set; }

        public override string ToString()
        {
            switch (What) {
                case Variant.NoChange:
                    return ""
                      + What + ": "
                      + long + " "
                      + "[" + Item + "] "
                      ;

                case Variant.CreateItem:
                case Variant.TouchItem:
                case Variant.DeleteItem:
                    return ""
                      + What + ": "
                      + long + " "
                      + "[" + Item + "] "
                      ;

                default:
                    return ""
                      + What + ": "
                      + long + " "
                      + "[" + Item + "] "
                      + Pid + " "
                      + PreviousValue + " -> "
                      + Value + " "
                      + ChildId
                      ;
            }
        }
    }
}
