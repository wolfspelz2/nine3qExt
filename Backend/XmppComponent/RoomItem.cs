namespace XmppComponent
{
    internal partial class Controller
    {
        public class RoomItem
        {
            public enum RezState
            {
                NoState,
                Dropping,
                Rezzing,
                Rezzed,
                Derezzing,
                Derezzed,
                Pickupping,
            }

            public readonly long ItemId;
            public string Resource;
            public RezState State = RezState.NoState;

            public RoomItem(long itemId, string resource = "item")
            {
                ItemId = itemId;
                Resource = resource;
            }
        }
    }
}