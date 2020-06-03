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

        public RezState State = RezState.NoState;

        public readonly string RoomId;
        public readonly string ItemId;

        public RoomItem(string roomId, string itemId)
        {
            RoomId = roomId;
            ItemId = itemId;
        }
    }
}