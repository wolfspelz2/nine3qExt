using nine3q.Tools;

namespace XmppComponent
{
    public class RoomItemJid
    {
        public string Room { get; set; }
        public long Item { get; set; }
        public string Name { get; set; }
        public string Resource => $"{Name} {Item}";
        public string Full => $"{Room}/{Full}";

        public RoomItemJid(string roomId, long itemId, string itemName)
        {
            Room = roomId;
            Item = itemId;
            Name = itemName;
        }

        public RoomItemJid(string full)
        {
            var jid = new XmppJid(full);
            Room = jid.User + "@" + jid.Domain;
            var itemNameAndId = jid.Resource;
            var parts = itemNameAndId.Split(new char[] { ' ' });
            var itemIdStr = parts.Length > 0 ? parts[parts.Length - 1] : "";
            var itemId = (long)0;
            _ = long.TryParse(itemIdStr, out itemId);
            Item = itemId;
            Name = itemNameAndId.Substring(0, itemNameAndId.Length - itemIdStr.Length - 1);
        }
    }
}
