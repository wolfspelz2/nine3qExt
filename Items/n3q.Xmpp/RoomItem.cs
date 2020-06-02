using n3q.Items;

namespace XmppComponent
{
    public class RoomItem
    {
        public enum RezState
        {
            NoState,
            Rezzing,
            Rezzed,
            Derezzing,
        }

        public string Resource;
        public RezState State = RezState.NoState;

        public readonly string RoomId;
        public readonly string ItemId;

        public RoomItem(string roomId, string itemId, string resource = "item")
        {
            RoomId = roomId;
            ItemId = itemId;
            Resource = resource;
        }
    }
}