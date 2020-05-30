using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XmppComponent.Test
{
    [TestClass]
    public class RoomItemJidTest
    {
        [TestMethod]
        public void From_full_JID()
        {
            Assert.AreEqual("fce62790a52fcb799c8a1835bb21c3262ac009d9@muc4.virtual-presence.org", new RoomItemJid("fce62790a52fcb799c8a1835bb21c3262ac009d9@muc4.virtual-presence.org/12345").Room);
            Assert.AreEqual("12345", new RoomItemJid("fce62790a52fcb799c8a1835bb21c3262ac009d9@muc4.virtual-presence.org/12345").Item);
            Assert.AreEqual("12345", new RoomItemJid("fce62790a52fcb799c8a1835bb21c3262ac009d9@muc4.virtual-presence.org/12345").Resource);

            //Assert.AreEqual("fce62790a52fcb799c8a1835bb21c3262ac009d9@muc4.virtual-presence.org", new RoomItemJid("fce62790a52fcb799c8a1835bb21c3262ac009d9@muc4.virtual-presence.org/General Sherman 12345").Room);
            //Assert.AreEqual("General Sherman", new RoomItemJid("fce62790a52fcb799c8a1835bb21c3262ac009d9@muc4.virtual-presence.org/General Sherman 12345").Name);
            //Assert.AreEqual(12345, new RoomItemJid("fce62790a52fcb799c8a1835bb21c3262ac009d9@muc4.virtual-presence.org/General Sherman 12345").Item);
            //Assert.AreEqual("General Sherman 12345", new RoomItemJid("fce62790a52fcb799c8a1835bb21c3262ac009d9@muc4.virtual-presence.org/General Sherman 12345").Resource);
        }

        [TestMethod]
        public void From_parts()
        {
            Assert.AreEqual("fce62790a52fcb799c8a1835bb21c3262ac009d9@muc4.virtual-presence.org", new RoomItemJid("fce62790a52fcb799c8a1835bb21c3262ac009d9@muc4.virtual-presence.org", "12345", "General Sherman").Room);
            Assert.AreEqual("12345", new RoomItemJid("fce62790a52fcb799c8a1835bb21c3262ac009d9@muc4.virtual-presence.org", "12345", "General Sherman").Item);
            Assert.AreEqual("12345", new RoomItemJid("fce62790a52fcb799c8a1835bb21c3262ac009d9@muc4.virtual-presence.org", "12345", "General Sherman").Resource);

            //Assert.AreEqual("fce62790a52fcb799c8a1835bb21c3262ac009d9@muc4.virtual-presence.org", new RoomItemJid("fce62790a52fcb799c8a1835bb21c3262ac009d9@muc4.virtual-presence.org", "12345", "General Sherman").Room);
            //Assert.AreEqual(12345, new RoomItemJid("fce62790a52fcb799c8a1835bb21c3262ac009d9@muc4.virtual-presence.org", "12345", "General Sherman").Item);
            //Assert.AreEqual("General Sherman", new RoomItemJid("fce62790a52fcb799c8a1835bb21c3262ac009d9@muc4.virtual-presence.org", "12345", "General Sherman").Name);
            //Assert.AreEqual("General Sherman 12345", new RoomItemJid("fce62790a52fcb799c8a1835bb21c3262ac009d9@muc4.virtual-presence.org", "12345", "General Sherman").Resource);
        }

        [TestMethod]
        public void To_full_JID()
        {
            Assert.AreEqual("fce62790a52fcb799c8a1835bb21c3262ac009d9@muc4.virtual-presence.org/12345", new RoomItemJid("fce62790a52fcb799c8a1835bb21c3262ac009d9@muc4.virtual-presence.org", "12345", "General Sherman").Full);

            //Assert.AreEqual("fce62790a52fcb799c8a1835bb21c3262ac009d9@muc4.virtual-presence.org/General Sherman 12345", new RoomItemJid("fce62790a52fcb799c8a1835bb21c3262ac009d9@muc4.virtual-presence.org", 12345, "General Sherman").Full);
        }
    }
}
