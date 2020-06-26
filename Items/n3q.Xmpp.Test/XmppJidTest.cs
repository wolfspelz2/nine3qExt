using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace n3q.Xmpp.Test
{
    [TestClass]
    public class XmppJidTest
    {
        [TestMethod]
        public void Parse()
        {
            {
                var jid = "xmpp:user@domain.com/res";
                Assert.AreEqual("xmpp", new XmppJid(jid).Scheme);
                Assert.AreEqual("user", new XmppJid(jid).User);
                Assert.AreEqual("domain.com", new XmppJid(jid).Domain);
                Assert.AreEqual("res", new XmppJid(jid).Resource);
                Assert.AreEqual("user@domain.com", new XmppJid(jid).Base);
                Assert.AreEqual("user@domain.com/res", new XmppJid(jid).Full);
                Assert.AreEqual("xmpp:user@domain.com/res", new XmppJid(jid).URI);
            }

            {
                var jid = "jabber:user@domain.com/res";
                Assert.AreEqual("xmpp", new XmppJid(jid).Scheme);
                Assert.AreEqual("user", new XmppJid(jid).User);
                Assert.AreEqual("domain.com", new XmppJid(jid).Domain);
                Assert.AreEqual("res", new XmppJid(jid).Resource);
                Assert.AreEqual("user@domain.com", new XmppJid(jid).Base);
                Assert.AreEqual("user@domain.com/res", new XmppJid(jid).Full);
                Assert.AreEqual("xmpp:user@domain.com/res", new XmppJid(jid).URI);
            }

            {
                var jid = "user@domain.com/res";
                Assert.AreEqual("xmpp", new XmppJid(jid).Scheme);
                Assert.AreEqual("user", new XmppJid(jid).User);
                Assert.AreEqual("domain.com", new XmppJid(jid).Domain);
                Assert.AreEqual("res", new XmppJid(jid).Resource);
                Assert.AreEqual("user@domain.com", new XmppJid(jid).Base);
                Assert.AreEqual(jid, new XmppJid(jid).Full);
                Assert.AreEqual("xmpp:user@domain.com/res", new XmppJid(jid).URI);
            }

            {
                var jid = "user@domain.com";
                Assert.AreEqual("xmpp", new XmppJid(jid).Scheme);
                Assert.AreEqual("user", new XmppJid(jid).User);
                Assert.AreEqual("domain.com", new XmppJid(jid).Domain);
                Assert.AreEqual("", new XmppJid(jid).Resource);
                Assert.AreEqual(jid, new XmppJid(jid).Base);
                Assert.AreEqual(jid, new XmppJid(jid).Full);
                Assert.AreEqual("xmpp:user@domain.com", new XmppJid(jid).URI);
            }

            {
                var jid = "domain.com";
                Assert.AreEqual("xmpp", new XmppJid(jid).Scheme);
                Assert.AreEqual("", new XmppJid(jid).User);
                Assert.AreEqual(jid, new XmppJid(jid).Domain);
                Assert.AreEqual("", new XmppJid(jid).Resource);
                Assert.AreEqual(jid, new XmppJid(jid).Base);
                Assert.AreEqual(jid, new XmppJid(jid).Full);
                Assert.AreEqual("xmpp:domain.com", new XmppJid(jid).URI);
            }
        }

    }
}
