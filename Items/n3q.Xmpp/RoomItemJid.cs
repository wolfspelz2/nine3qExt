using n3q.Tools;

namespace XmppComponent
{
    public class RoomItemJid
    {
        public string Room { get; set; }
        public string Item { get; set; }
        public string Name { get; set; }
        public string Resource => Item;//$"{Name} {Item}";
        public string Full => $"{Room}/{Resource}";

        public RoomItemJid(string roomId, string itemId, string itemName)
        {
            Room = roomId;
            Item = itemId;
            Name = itemName;
        }

        public RoomItemJid(string full)
        {
            var jid = new XmppJid(full);
            Room = jid.User + "@" + jid.Domain;
            Item = jid.Resource;
            //var itemNameAndId = jid.Resource;
            //var parts = itemNameAndId.Split(new char[] { ' ' });
            //Item = parts.Length > 0 ? parts[parts.Length - 1] : "";
            //var len = itemNameAndId.Length - Item.Length - 1;
            //if (len >= 0) {
            //    Name = itemNameAndId.Substring(0, len);
            //}
        }
    }
}
