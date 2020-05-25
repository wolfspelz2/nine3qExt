using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XmppComponent.Test
{
    [TestClass]
    public class XmppJidTest
    {
        [TestMethod]
        public void Parse()
        {
            Assert.AreEqual("xmpp", new XmppJid("xmpp:user@domain.com/res").Scheme);
            Assert.AreEqual("user", new XmppJid("xmpp:user@domain.com/res").User);
            Assert.AreEqual("domain.com", new XmppJid("xmpp:user@domain.com/res").Domain);
            Assert.AreEqual("res", new XmppJid("xmpp:user@domain.com/res").Resource);

            Assert.AreEqual("xmpp", new XmppJid("jabber:user@domain.com/res").Scheme);
            Assert.AreEqual("user", new XmppJid("jabber:user@domain.com/res").User);
            Assert.AreEqual("domain.com", new XmppJid("jabber:user@domain.com/res").Domain);
            Assert.AreEqual("res", new XmppJid("jabber:user@domain.com/res").Resource);

            Assert.AreEqual("xmpp", new XmppJid("user@domain.com/res").Scheme);
            Assert.AreEqual("user", new XmppJid("user@domain.com/res").User);
            Assert.AreEqual("domain.com", new XmppJid("user@domain.com/res").Domain);
            Assert.AreEqual("res", new XmppJid("user@domain.com/res").Resource);

            Assert.AreEqual("xmpp", new XmppJid("user@domain.com").Scheme);
            Assert.AreEqual("user", new XmppJid("user@domain.com").User);
            Assert.AreEqual("domain.com", new XmppJid("user@domain.com").Domain);
            Assert.AreEqual("", new XmppJid("user@domain.com").Resource);

            Assert.AreEqual("xmpp", new XmppJid("res").Scheme);
            Assert.AreEqual("", new XmppJid("res").User);
            Assert.AreEqual("res", new XmppJid("res").Domain);
            Assert.AreEqual("", new XmppJid("res").Resource);

            Assert.AreEqual("xmpp", new XmppJid("domain.com/res").Scheme);
            Assert.AreEqual("", new XmppJid("domain.com/res").User);
            Assert.AreEqual("domain.com", new XmppJid("domain.com/res").Domain);
            Assert.AreEqual("res", new XmppJid("domain.com/res").Resource);

        }

    }
}
