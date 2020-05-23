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
        public readonly long ItemId;

        public RoomItem(string roomId, long itemId, string resource = "item")
        {
            RoomId = roomId;
            ItemId = itemId;
            Resource = resource;
        }
    }
}