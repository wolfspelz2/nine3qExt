using n3q.Items;

namespace XmppComponent
{
    public class InventoryItem
    {
        public readonly string InventoryId;
        public readonly string ItemId;

        public InventoryItem(string inventoryId, string itemId)
        {
            InventoryId = inventoryId;
            ItemId = itemId;
        }
    }
}