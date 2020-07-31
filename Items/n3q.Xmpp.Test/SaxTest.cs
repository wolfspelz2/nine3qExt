using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace n3q.Xmpp.Test
{
    [TestClass]
    public class SaxTest
    {
        //        readonly string _componentStream = @"<?xml version='1.0'?>
        //<stream:stream xml:lang='en' id='7bb836d7-a0e1-4e88-b7a9-90c648c74235' xmlns='jabber:component:accept' from='itemsxmpp.dev.sui.li' xmlns:stream='http://etherx.jabber.org/streams'>
        //<handshake/>
        //<presence from='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org/bh1' to='bh1@itemsxmpp.dev.sui.li'><x ImageUrl='{image.item.nine3q}System/Blackhole.png' ApplierAspect='True' Container='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org' Label='Blackhole' InventoryX='336' Width='100' InventoryY='121' xmlns='vp:props' type='item' Name='Blackhole' provider='nine3q' Height='70' RezzedX='232'/><x xmlns='firebat:user:identity' jid='bh1@itemsxmpp.dev.sui.li' src='http://localhost:5001/Identity/Generated?avatarUrl=&amp;nickname=Blackhole&amp;digest=314951952&amp;imageUrl=%7Bimage.item.nine3q%7DSystem%2FBlackhole.png' digest='314951952'/><x xmlns='firebat:avatar:state'><position x='232'/></x><x xmlns='http://jabber.org/protocol/muc#user'><item role='moderator' jid='bh1@itemsxmpp.dev.sui.li' affiliation='owner'/><status code='201'/></x></presence><presence from='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org/bh1' to='scriptgen1@itemsxmpp.dev.sui.li'><x ImageUrl='{image.item.nine3q}System/Blackhole.png' ApplierAspect='True' Container='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org' Label='Blackhole' InventoryX='336' Width='100' InventoryY='121' xmlns='vp:props' type='item' Name='Blackhole' provider='nine3q' Height='70' RezzedX='232'/><x xmlns='firebat:user:identity' jid='bh1@itemsxmpp.dev.sui.li' src='http://localhost:5001/Identity/Generated?avatarUrl=&amp;nickname=Blackhole&amp;digest=314951952&amp;imageUrl=%7Bimage.item.nine3q%7DSystem%2FBlackhole.png' digest='314951952'/><x xmlns='firebat:avatar:state'><position x='232'/></x><x xmlns='http://jabber.org/protocol/muc#user'><item role='moderator' affiliation='owner'/></x></presence><presence from='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org/scriptgen1' to='scriptgen1@itemsxmpp.dev.sui.li'><x ImageUrl='{image.item.nine3q}TheatreScreenplay/TheatreScreenplayDispenser.png' Container='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org' DispenserLastTime='120200728163017583' Label='TheatreScreenplayDispenser' InventoryX='203' Width='78' Height='84' provider='nine3q' IframeAspect='True' IframeWidth='100' DispenserMaxAvailable='1000' xmlns='vp:props' IframeUrl='{iframe.item.nine3q}TheatreScreenplayDispenser?context={context}' IframeResizeable='True' InventoryY='120' DispenserAvailable='967' DispenserCooldownSec='10' IframeHeight='100' Name='TheatreScreenplayDispenser' IframeFrame='Popup' RezzedX='115' type='item'/><x xmlns='firebat:user:identity' jid='scriptgen1@itemsxmpp.dev.sui.li' src='http://localhost:5001/Identity/Generated?avatarUrl=&amp;nickname=TheatreScreenplayDispenser&amp;digest=1703338711&amp;imageUrl=%7Bimage.item.nine3q%7DTheatreScreenplay%2FTheatreScreenplayDispenser.png' digest='1703338711'/><x xmlns='firebat:avatar:state'><position x='115'/></x><x xmlns='http://jabber.org/protocol/muc#user'><item role='participant' affiliation='none'/></x></presence>
        //<presence from='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org/scriptgen1' to='bh1@itemsxmpp.dev.sui.li'>
        //    <x ImageUrl='{image.item.nine3q}TheatreScreenplay/TheatreScreenplayDispenser.png' Container='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org' DispenserLastTime='120200728163017583' Label='TheatreScreenplayDispenser' InventoryX='203' Width='78' Height='84' provider='nine3q' IframeAspect='True' IframeWidth='100' DispenserMaxAvailable='1000' xmlns='vp:props' IframeUrl='{iframe.item.nine3q}TheatreScreenplayDispenser?context={context}' IframeResizeable='True' InventoryY='120' DispenserAvailable='967' DispenserCooldownSec='10' IframeHeight='100' Name='TheatreScreenplayDispenser' IframeFrame='Popup' RezzedX='115' type='item'/>
        //    <x xmlns='firebat:user:identity' jid='scriptgen1@itemsxmpp.dev.sui.li' src='http://localhost:5001/Identity/Generated?avatarUrl=&amp;nickname=TheatreScreenplayDispenser&amp;digest=1703338711&amp;imageUrl=%7Bimage.item.nine3q%7DTheatreScreenplay%2FTheatreScreenplayDispenser.png' digest='1703338711'/>
        //    <x xmlns='firebat:avatar:state'>
        //        <position x='115'/>
        //    </x>
        //    <x xmlns='http://jabber.org/protocol/muc#user'>
        //        <item role='participant' jid='scriptgen1@itemsxmpp.dev.sui.li' affiliation='none'/>
        //    </x>
        //</presence>
        //<presence from='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org/scriptgen1' to='mh1@itemsxmpp.dev.sui.li'><x ImageUrl='{image.item.nine3q}TheatreScreenplay/TheatreScreenplayDispenser.png' Container='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org' DispenserLastTime='120200728163017583' Label='TheatreScreenplayDispenser' InventoryX='203' Width='78' Height='84' provider='nine3q' IframeAspect='True' IframeWidth='100' DispenserMaxAvailable='1000' xmlns='vp:props' IframeUrl='{iframe.item.nine3q}TheatreScreenplayDispenser?context={context}' IframeResizeable='True' InventoryY='120' DispenserAvailable='967' DispenserCooldownSec='10' IframeHeight='100' Name='TheatreScreenplayDispenser' IframeFrame='Popup' RezzedX='115' type='item'/><x xmlns='firebat:user:identity' jid='scriptgen1@itemsxmpp.dev.sui.li' src='http://localhost:5001/Identity/Generated?avatarUrl=&amp;nickname=TheatreScreenplayDispenser&amp;digest=1703338711&amp;imageUrl=%7Bimage.item.nine3q%7DTheatreScreenplay%2FTheatreScreenplayDispenser.png' digest='1703338711'/><x xmlns='firebat:avatar:state'><position x='115'/></x><x xmlns='http://jabber.org/protocol/muc#user'><item role='participant' affiliation='none'/></x></presence><presence from='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org/bh1' to='mh1@itemsxmpp.dev.sui.li'><x ImageUrl='{image.item.nine3q}System/Blackhole.png' ApplierAspect='True' Container='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org' Label='Blackhole' InventoryX='336' Width='100' InventoryY='121' xmlns='vp:props' type='item' Name='Blackhole' provider='nine3q' Height='70' RezzedX='232'/><x xmlns='firebat:user:identity' jid='bh1@itemsxmpp.dev.sui.li' src='http://localhost:5001/Identity/Generated?avatarUrl=&amp;nickname=Blackhole&amp;digest=314951952&amp;imageUrl=%7Bimage.item.nine3q%7DSystem%2FBlackhole.png' digest='314951952'/><x xmlns='firebat:avatar:state'><position x='232'/></x><x xmlns='http://jabber.org/protocol/muc#user'><item role='moderator' affiliation='owner'/></x></presence><presence from='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org/mh1' to='scriptgen1@itemsxmpp.dev.sui.li'><x ImageUrl='{image.item.nine3q}System/MagicHat.png' Container='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org' IframeUrl='{iframe.item.nine3q}MagicHat?context={context}' Label='MagicHat' InventoryX='124' Width='80' Height='58' provider='nine3q' IframeAspect='True' IframeWidth='200' InventoryY='53' xmlns='vp:props' RezzedX='417' IframeFrame='Popup' Name='MagicHat' IframeHeight='105' IframeResizeable='True' type='item'/><x xmlns='firebat:user:identity' jid='mh1@itemsxmpp.dev.sui.li' src='http://localhost:5001/Identity/Generated?avatarUrl=&amp;nickname=MagicHat&amp;digest=2091463095&amp;imageUrl=%7Bimage.item.nine3q%7DSystem%2FMagicHat.png' digest='2091463095'/><x xmlns='firebat:avatar:state'><position x='417'/></x><x xmlns='http://jabber.org/protocol/muc#user'><item role='participant' affiliation='none'/></x></presence><presence from='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org/mh1' to='bh1@itemsxmpp.dev.sui.li'><x ImageUrl='{image.item.nine3q}System/MagicHat.png' Container='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org' IframeUrl='{iframe.item.nine3q}MagicHat?context={context}' Label='MagicHat' InventoryX='124' Width='80' Height='58' provider='nine3q' IframeAspect='True' IframeWidth='200' InventoryY='53' xmlns='vp:props' RezzedX='417' IframeFrame='Popup' Name='MagicHat' IframeHeight='105' IframeResizeable='True' type='item'/><x xmlns='firebat:user:identity' jid='mh1@itemsxmpp.dev.sui.li' src='http://localhost:5001/Identity/Generated?avatarUrl=&amp;nickname=MagicHat&amp;digest=2091463095&amp;imageUrl=%7Bimage.item.nine3q%7DSystem%2FMagicHat.png' digest='2091463095'/><x xmlns='firebat:avatar:state'><position x='417'/></x><x xmlns='http://jabber.org/protocol/muc#user'><item role='participant' jid='mh1@itemsxmpp.dev.sui.li' affiliation='none'/></x></presence><presence from='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org/mh1' to='mh1@itemsxmpp.dev.sui.li'><x ImageUrl='{image.item.nine3q}System/MagicHat.png' Container='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org' IframeUrl='{iframe.item.nine3q}MagicHat?context={context}' Label='MagicHat' InventoryX='124' Width='80' Height='58' provider='nine3q' IframeAspect='True' IframeWidth='200' InventoryY='53' xmlns='vp:props' RezzedX='417' IframeFrame='Popup' Name='MagicHat' IframeHeight='105' IframeResizeable='True' type='item'/><x xmlns='firebat:user:identity' jid='mh1@itemsxmpp.dev.sui.li' src='http://localhost:5001/Identity/Generated?avatarUrl=&amp;nickname=MagicHat&amp;digest=2091463095&amp;imageUrl=%7Bimage.item.nine3q%7DSystem%2FMagicHat.png' digest='2091463095'/><x xmlns='firebat:avatar:state'><position x='417'/></x><x xmlns='http://jabber.org/protocol/muc#user'><item role='participant' affiliation='none'/></x></presence>
        //";
        [TestMethod]
        public void Basic()
        {
            // Arrange
            var xml = "<tag a1='v1' a2=\"&amp;v2&quot;\" a3>&amp;text</tag>";

            var nodeStart = 0;
            var nodeEnd = 0;
            var tagName = "";
            var tagAttributes = new Dictionary<string, string>();
            var tagText = "";
            var sax = new Sax();
            sax.NodeStart += (s, e) => { nodeStart++; tagName = e.Name; tagAttributes = e.Attributes; };
            sax.NodeEnd += (s, e) => { nodeEnd++; tagText = e.Text; };
            sax.ParseError += (s, e) => { throw new System.Exception($"line={e.Line} col={e.Column}"); };

            // Act
            sax.Parse(xml);

            // Assert
            Assert.AreEqual(1, nodeStart);
            Assert.AreEqual(1, nodeEnd);
            Assert.AreEqual("tag", tagName);
            Assert.AreEqual("&text", tagText);
            Assert.AreEqual(3, tagAttributes.Count);
            Assert.AreEqual("v1", tagAttributes["a1"]);
            Assert.AreEqual("&v2\"", tagAttributes["a2"]);
            Assert.AreEqual("", tagAttributes["a3"]);
        }

        [TestMethod]
        public void SelfClosing()
        {
            // Arrange
            var xml = "<tag a1='v1' a2=\"v2\" a3/>";

            var nodeStart = 0;
            var nodeEnd = 0;
            var tagName = "";
            var attributes = new Dictionary<string, string>();
            var sax = new Sax();
            sax.NodeStart += (s, e) => { nodeStart++; tagName = e.Name; attributes = e.Attributes; };
            sax.NodeEnd += (s, e) => { nodeEnd++; };
            sax.ParseError += (s, e) => { throw new System.Exception($"line={e.Line} col={e.Column}"); };

            // Act
            sax.Parse(xml);

            // Assert
            Assert.AreEqual(1, nodeStart);
            Assert.AreEqual(1, nodeEnd);
            Assert.AreEqual("tag", tagName);
            Assert.AreEqual(3, attributes.Count);
            Assert.AreEqual("", attributes["a3"]);
        }

        [TestMethod]
        public void Hierarchical()
        {
            // Arrange
            var xml = "";
            xml += "<n1 a11='v11' a12='v12'>\n";
            xml += "t1a\n";
            xml += "<n2 a21='v21'\n";
            xml += "t2\n";
            xml += "</n2>\n";
            xml += "t1b\n";
            xml += "</n1>\n";

            var nodeStart = 0;
            var nodeEnd = 0;
            var tagList = new List<string>();
            var attributesList = new List<Dictionary<string, string>>();
            var sax = new Sax();
            sax.NodeStart += (s, e) => { nodeStart++; tagList.Add(e.Name); attributesList.Add(e.Attributes); };
            sax.NodeEnd += (s, e) => { nodeEnd++; };
            sax.ParseError += (s, e) => { throw new System.Exception($"line={e.Line} col={e.Column}"); };

            // Act
            sax.Parse(xml);

            // Assert
            Assert.AreEqual(2, nodeStart);
            Assert.AreEqual(2, nodeEnd);
            Assert.AreEqual("n1", tagList[0]);
            Assert.AreEqual("n2", tagList[1]);
            Assert.AreEqual(2, attributesList[0].Count);
            Assert.AreEqual(1, attributesList[1].Count);
            Assert.AreEqual("v11", attributesList[0]["a11"]);
            Assert.AreEqual("v12", attributesList[0]["a12"]);
            Assert.AreEqual("v21", attributesList[1]["a21"]);
        }

        [TestMethod]
        public void Preamble()
        {
            // Arrange
            var xml = "<?xml version='1.0'?>\r\n<tag a='b'>text</tag>";

            var nodeStart = 0;
            var nodeEnd = 0;
            var preName = "";
            var preAttributes = new Dictionary<string, string>();
            var tagName = "";
            var tagAttributes = new Dictionary<string, string>();
            var tagText = "";
            var sax = new Sax();
            sax.Preamble += (s, e) => { preName = e.Name; preAttributes = e.Attributes; };
            sax.NodeStart += (s, e) => { nodeStart++; tagName = e.Name; tagAttributes = e.Attributes; };
            sax.NodeEnd += (s, e) => { nodeEnd++; tagText = e.Text; };
            sax.ParseError += (s, e) => { throw new System.Exception($"line={e.Line} col={e.Column}"); };

            // Act
            sax.Parse(xml);

            // Assert
            Assert.AreEqual("?xml", preName);
            Assert.AreEqual(1, preAttributes.Count);
            Assert.AreEqual("1.0", preAttributes["version"]);

            Assert.AreEqual(1, nodeStart);
            Assert.AreEqual(1, nodeEnd);

            Assert.AreEqual("tag", tagName);
            Assert.AreEqual("text", tagText);

            Assert.AreEqual(1, tagAttributes.Count);
            Assert.AreEqual("b", tagAttributes["a"]);
        }

        [TestMethod]
        public void ComponentStream()
        {
            // Arrange
            string xml = "";
            xml += "<?xml version=\"1.0\"?>\r\n";
            xml += "<stream:stream xml:lang=\"en\" id=\"7bb836d7-a0e1-4e88-b7a9-90c648c74235\" xmlns=\"jabber:component:accept\" from=\"itemsxmpp.dev.sui.li\" xmlns:stream=\"http://etherx.jabber.org/streams\">\r\n";
            xml += "<handshake/>\r\n";
            xml += "<presence from='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org/bh1' to='bh1@itemsxmpp.dev.sui.li'><x ImageUrl='{image.item.nine3q}System/Blackhole.png' ApplierAspect='True' Container='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org' Label='Blackhole' InventoryX='336' Width='100' InventoryY='121' xmlns='vp:props' type='item' Name='Blackhole' provider='nine3q' Height='70' RezzedX='232'/><x xmlns='firebat:user:identity' jid='bh1@itemsxmpp.dev.sui.li' src='http://localhost:5001/Identity/Generated?avatarUrl=&amp;nickname=Blackhole&amp;digest=314951952&amp;imageUrl=%7Bimage.item.nine3q%7DSystem%2FBlackhole.png' digest='314951952'/><x xmlns='firebat:avatar:state'><position x='232'/></x><x xmlns='http://jabber.org/protocol/muc#user'><item role='moderator' jid='bh1@itemsxmpp.dev.sui.li' affiliation='owner'/><status code='201'/></x></presence><presence from='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org/bh1' to='scriptgen1@itemsxmpp.dev.sui.li'><x ImageUrl='{image.item.nine3q}System/Blackhole.png' ApplierAspect='True' Container='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org' Label='Blackhole' InventoryX='336' Width='100' InventoryY='121' xmlns='vp:props' type='item' Name='Blackhole' provider='nine3q' Height='70' RezzedX='232'/><x xmlns='firebat:user:identity' jid='bh1@itemsxmpp.dev.sui.li' src='http://localhost:5001/Identity/Generated?avatarUrl=&amp;nickname=Blackhole&amp;digest=314951952&amp;imageUrl=%7Bimage.item.nine3q%7DSystem%2FBlackhole.png' digest='314951952'/><x xmlns='firebat:avatar:state'><position x='232'/></x><x xmlns='http://jabber.org/protocol/muc#user'><item role='moderator' affiliation='owner'/></x></presence><presence from='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org/scriptgen1' to='scriptgen1@itemsxmpp.dev.sui.li'><x ImageUrl='{image.item.nine3q}TheatreScreenplay/TheatreScreenplayDispenser.png' Container='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org' DispenserLastTime='120200728163017583' Label='TheatreScreenplayDispenser' InventoryX='203' Width='78' Height='84' provider='nine3q' IframeAspect='True' IframeWidth='100' DispenserMaxAvailable='1000' xmlns='vp:props' IframeUrl='{iframe.item.nine3q}TheatreScreenplayDispenser?context={context}' IframeResizeable='True' InventoryY='120' DispenserAvailable='967' DispenserCooldownSec='10' IframeHeight='100' Name='TheatreScreenplayDispenser' IframeFrame='Popup' RezzedX='115' type='item'/><x xmlns='firebat:user:identity' jid='scriptgen1@itemsxmpp.dev.sui.li' src='http://localhost:5001/Identity/Generated?avatarUrl=&amp;nickname=TheatreScreenplayDispenser&amp;digest=1703338711&amp;imageUrl=%7Bimage.item.nine3q%7DTheatreScreenplay%2FTheatreScreenplayDispenser.png' digest='1703338711'/><x xmlns='firebat:avatar:state'><position x='115'/></x><x xmlns='http://jabber.org/protocol/muc#user'><item role='participant' affiliation='none'/></x></presence>\r\n";
            xml += "<presence from=\"d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org/scriptgen1\" to=\"bh1@itemsxmpp.dev.sui.li\">\r\n";
            xml += "    <x ImageUrl=\"{image.item.nine3q}TheatreScreenplay/TheatreScreenplayDispenser.png\" Container=\"d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org\" DispenserLastTime=\"120200728163017583\" Label=\"TheatreScreenplayDispenser\" InventoryX=\"203\" Width=\"78\" Height=\"84\" provider=\"nine3q\" IframeAspect=\"True\" IframeWidth=\"100\" DispenserMaxAvailable=\"1000\" xmlns=\"vp:props\" IframeUrl=\"{iframe.item.nine3q}TheatreScreenplayDispenser?context={context}\" IframeResizeable=\"True\" InventoryY=\"120\" DispenserAvailable=\"967\" DispenserCooldownSec=\"10\" IframeHeight=\"100\" Name=\"TheatreScreenplayDispenser\" IframeFrame=\"Popup\" RezzedX=\"115\" type=\"item\"/>\r\n";
            xml += "    <x xmlns=\"firebat:user:identity\" jid=\"scriptgen1@itemsxmpp.dev.sui.li\" src=\"http://localhost:5001/Identity/Generated?avatarUrl=&amp;nickname=TheatreScreenplayDispenser&amp;digest=1703338711&amp;imageUrl=%7Bimage.item.nine3q%7DTheatreScreenplay%2FTheatreScreenplayDispenser.png\" digest=\"1703338711\"/>\r\n";
            xml += "    <x xmlns=\"firebat:avatar:state\">\r\n";
            xml += "        <position x=\"115\"/>\r\n";
            xml += "    </x>\r\n";
            xml += "    <x xmlns=\"http://jabber.org/protocol/muc#user\">\r\n";
            xml += "        <item role=\"participant\" jid=\"scriptgen1@itemsxmpp.dev.sui.li\" affiliation=\"none\"/>\r\n";
            xml += "    </x>\r\n";
            xml += "</presence>\r\n";
            xml += "<presence from='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org/scriptgen1' to='mh1@itemsxmpp.dev.sui.li'><x ImageUrl='{image.item.nine3q}TheatreScreenplay/TheatreScreenplayDispenser.png' Container='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org' DispenserLastTime='120200728163017583' Label='TheatreScreenplayDispenser' InventoryX='203' Width='78' Height='84' provider='nine3q' IframeAspect='True' IframeWidth='100' DispenserMaxAvailable='1000' xmlns='vp:props' IframeUrl='{iframe.item.nine3q}TheatreScreenplayDispenser?context={context}' IframeResizeable='True' InventoryY='120' DispenserAvailable='967' DispenserCooldownSec='10' IframeHeight='100' Name='TheatreScreenplayDispenser' IframeFrame='Popup' RezzedX='115' type='item'/><x xmlns='firebat:user:identity' jid='scriptgen1@itemsxmpp.dev.sui.li' src='http://localhost:5001/Identity/Generated?avatarUrl=&amp;nickname=TheatreScreenplayDispenser&amp;digest=1703338711&amp;imageUrl=%7Bimage.item.nine3q%7DTheatreScreenplay%2FTheatreScreenplayDispenser.png' digest='1703338711'/><x xmlns='firebat:avatar:state'><position x='115'/></x><x xmlns='http://jabber.org/protocol/muc#user'><item role='participant' affiliation='none'/></x></presence><presence from='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org/bh1' to='mh1@itemsxmpp.dev.sui.li'><x ImageUrl='{image.item.nine3q}System/Blackhole.png' ApplierAspect='True' Container='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org' Label='Blackhole' InventoryX='336' Width='100' InventoryY='121' xmlns='vp:props' type='item' Name='Blackhole' provider='nine3q' Height='70' RezzedX='232'/><x xmlns='firebat:user:identity' jid='bh1@itemsxmpp.dev.sui.li' src='http://localhost:5001/Identity/Generated?avatarUrl=&amp;nickname=Blackhole&amp;digest=314951952&amp;imageUrl=%7Bimage.item.nine3q%7DSystem%2FBlackhole.png' digest='314951952'/><x xmlns='firebat:avatar:state'><position x='232'/></x><x xmlns='http://jabber.org/protocol/muc#user'><item role='moderator' affiliation='owner'/></x></presence><presence from='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org/mh1' to='scriptgen1@itemsxmpp.dev.sui.li'><x ImageUrl='{image.item.nine3q}System/MagicHat.png' Container='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org' IframeUrl='{iframe.item.nine3q}MagicHat?context={context}' Label='MagicHat' InventoryX='124' Width='80' Height='58' provider='nine3q' IframeAspect='True' IframeWidth='200' InventoryY='53' xmlns='vp:props' RezzedX='417' IframeFrame='Popup' Name='MagicHat' IframeHeight='105' IframeResizeable='True' type='item'/><x xmlns='firebat:user:identity' jid='mh1@itemsxmpp.dev.sui.li' src='http://localhost:5001/Identity/Generated?avatarUrl=&amp;nickname=MagicHat&amp;digest=2091463095&amp;imageUrl=%7Bimage.item.nine3q%7DSystem%2FMagicHat.png' digest='2091463095'/><x xmlns='firebat:avatar:state'><position x='417'/></x><x xmlns='http://jabber.org/protocol/muc#user'><item role='participant' affiliation='none'/></x></presence><presence from='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org/mh1' to='bh1@itemsxmpp.dev.sui.li'><x ImageUrl='{image.item.nine3q}System/MagicHat.png' Container='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org' IframeUrl='{iframe.item.nine3q}MagicHat?context={context}' Label='MagicHat' InventoryX='124' Width='80' Height='58' provider='nine3q' IframeAspect='True' IframeWidth='200' InventoryY='53' xmlns='vp:props' RezzedX='417' IframeFrame='Popup' Name='MagicHat' IframeHeight='105' IframeResizeable='True' type='item'/><x xmlns='firebat:user:identity' jid='mh1@itemsxmpp.dev.sui.li' src='http://localhost:5001/Identity/Generated?avatarUrl=&amp;nickname=MagicHat&amp;digest=2091463095&amp;imageUrl=%7Bimage.item.nine3q%7DSystem%2FMagicHat.png' digest='2091463095'/><x xmlns='firebat:avatar:state'><position x='417'/></x><x xmlns='http://jabber.org/protocol/muc#user'><item role='participant' jid='mh1@itemsxmpp.dev.sui.li' affiliation='none'/></x></presence><presence from='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org/mh1' to='mh1@itemsxmpp.dev.sui.li'><x ImageUrl='{image.item.nine3q}System/MagicHat.png' Container='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org' IframeUrl='{iframe.item.nine3q}MagicHat?context={context}' Label='MagicHat' InventoryX='124' Width='80' Height='58' provider='nine3q' IframeAspect='True' IframeWidth='200' InventoryY='53' xmlns='vp:props' RezzedX='417' IframeFrame='Popup' Name='MagicHat' IframeHeight='105' IframeResizeable='True' type='item'/><x xmlns='firebat:user:identity' jid='mh1@itemsxmpp.dev.sui.li' src='http://localhost:5001/Identity/Generated?avatarUrl=&amp;nickname=MagicHat&amp;digest=2091463095&amp;imageUrl=%7Bimage.item.nine3q%7DSystem%2FMagicHat.png' digest='2091463095'/><x xmlns='firebat:avatar:state'><position x='417'/></x><x xmlns='http://jabber.org/protocol/muc#user'><item role='participant' affiliation='none'/></x></presence>\r\n";
            xml += "<presence from='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org/scriptgen1' to='pirzde9srwh9d5bup7k1hcm@itemsxmpp.dev.sui.li'><x ImageUrl='{image.item.nine3q}TheatreScreenplay/TheatreScreenplayDispenser.png' Container='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org' DispenserLastTime='120200728163017583' Label='TheatreScreenplayDispenser' InventoryX='203' Width='78' Height='84' provider='nine3q' IframeAspect='True' IframeWidth='100' DispenserMaxAvailable='1000' xmlns='vp:props' IframeUrl='{iframe.item.nine3q}TheatreScreenplayDispenser?context={context}' IframeResizeable='True' InventoryY='120' DispenserAvailable='967' DispenserCooldownSec='10' IframeHeight='100' Name='TheatreScreenplayDispenser' IframeFrame='Popup' RezzedX='115' type='item'/><x xmlns='firebat:user:identity' jid='scriptgen1@itemsxmpp.dev.sui.li' src='http://localhost:5001/Identity/Generated?avatarUrl=&amp;nickname=TheatreScreenplayDispenser&amp;digest=1703338711&amp;imageUrl=%7Bimage.item.nine3q%7DTheatreScreenplay%2FTheatreScreenplayDispenser.png' digest='1703338711'/><x xmlns='firebat:avatar:state'><position x='115'/></x><x xmlns='http://jabber.org/protocol/muc#user'><item role='participant' affiliation='none'/></x></presence><presence from='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org/bh1' to='pirzde9srwh9d5bup7k1hcm@itemsxmpp.dev.sui.li'><x ImageUrl='{image.item.nine3q}System/Blackhole.png' ApplierAspect='True' Container='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org' Label='Blackhole' InventoryX='336' Width='100' InventoryY='121' xmlns='vp:props' type='item' Name='Blackhole' provider='nine3q' Height='70' RezzedX='232'/><x xmlns='firebat:user:identity' jid='bh1@itemsxmpp.dev.sui.li' src='http://localhost:5001/Identity/Generated?avatarUrl=&amp;nickname=Blackhole&amp;digest=314951952&amp;imageUrl=%7Bimage.item.nine3q%7DSystem%2FBlackhole.png' digest='314951952'/><x xmlns='firebat:avatar:state'><position x='232'/></x><x xmlns='http://jabber.org/protocol/muc#user'><item role='moderator' affiliation='owner'/></x></presence><presence from='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org/mh1' to='pirzde9srwh9d5bup7k1hcm@itemsxmpp.dev.sui.li'><x ImageUrl='{image.item.nine3q}System/MagicHat.png' Container='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org' IframeUrl='{iframe.item.nine3q}MagicHat?context={context}' Label='MagicHat' InventoryX='124' Width='80' Height='58' provider='nine3q' IframeAspect='True' IframeWidth='200' InventoryY='53' xmlns='vp:props' RezzedX='417' IframeFrame='Popup' Name='MagicHat' IframeHeight='105' IframeResizeable='True' type='item'/><x xmlns='firebat:user:identity' jid='mh1@itemsxmpp.dev.sui.li' src='http://localhost:5001/Identity/Generated?avatarUrl=&amp;nickname=MagicHat&amp;digest=2091463095&amp;imageUrl=%7Bimage.item.nine3q%7DSystem%2FMagicHat.png' digest='2091463095'/><x xmlns='firebat:avatar:state'><position x='417'/></x><x xmlns='http://jabber.org/protocol/muc#user'><item role='participant' affiliation='none'/></x></presence><presence from='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org/pirzde9srwh9d5bup7k1hcm' to='scriptgen1@itemsxmpp.dev.sui.li'><x ImageUrl='{image.item.nine3q}PirateFlag/image.png' RezableAspect='True' Container='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org' Label='PirateFlag' Width='43' AnimationsUrl='{image.item.nine3q}PirateFlag/animations.xml' xmlns='vp:props' type='item' Height='65' Name='PirateFlag' provider='nine3q' PageClaimAspect='True' RezzedX='352'/><x xmlns='firebat:user:identity' jid='pirzde9srwh9d5bup7k1hcm@itemsxmpp.dev.sui.li' src='http://localhost:5001/Identity/Generated?avatarUrl=%7Bimage.item.nine3q%7DPirateFlag%2Fanimations.xml&amp;nickname=PirateFlag&amp;digest=1139695235&amp;imageUrl=%7Bimage.item.nine3q%7DPirateFlag%2Fimage.png' digest='1139695235'/><x xmlns='firebat:avatar:state'><position x='352'/></x><x xmlns='http://jabber.org/protocol/muc#user'><item role='participant' affiliation='none'/></x></presence><presence from='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org/pirzde9srwh9d5bup7k1hcm' to='bh1@itemsxmpp.dev.sui.li'><x ImageUrl='{image.item.nine3q}PirateFlag/image.png' RezableAspect='True' Container='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org' Label='PirateFlag' Width='43' AnimationsUrl='{image.item.nine3q}PirateFlag/animations.xml' xmlns='vp:props' type='item' Height='65' Name='PirateFlag' provider='nine3q' PageClaimAspect='True' RezzedX='352'/><x xmlns='firebat:user:identity' jid='pirzde9srwh9d5bup7k1hcm@itemsxmpp.dev.sui.li' src='http://localhost:5001/Identity/Generated?avatarUrl=%7Bimage.item.nine3q%7DPirateFlag%2Fanimations.xml&amp;nickname=PirateFlag&amp;digest=1139695235&amp;imageUrl=%7Bimage.item.nine3q%7DPirateFlag%2Fimage.png' digest='1139695235'/><x xmlns='firebat:avatar:state'><position x='352'/></x><x xmlns='http://jabber.org/protocol/muc#user'><item role='participant' jid='pirzde9srwh9d5bup7k1hcm@itemsxmpp.dev.sui.li' affiliation='none'/></x></presence><presence from='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org/pirzde9srwh9d5bup7k1hcm' to='mh1@itemsxmpp.dev.sui.li'><x ImageUrl='{image.item.nine3q}PirateFlag/image.png' RezableAspect='True' Container='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org' Label='PirateFlag' Width='43' AnimationsUrl='{image.item.nine3q}PirateFlag/animations.xml' xmlns='vp:props' type='item' Height='65' Name='PirateFlag' provider='nine3q' PageClaimAspect='True' RezzedX='352'/><x xmlns='firebat:user:identity' jid='pirzde9srwh9d5bup7k1hcm@itemsxmpp.dev.sui.li' src='http://localhost:5001/Identity/Generated?avatarUrl=%7Bimage.item.nine3q%7DPirateFlag%2Fanimations.xml&amp;nickname=PirateFlag&amp;digest=1139695235&amp;imageUrl=%7Bimage.item.nine3q%7DPirateFlag%2Fimage.png' digest='1139695235'/><x xmlns='firebat:avatar:state'><position x='352'/></x><x xmlns='http://jabber.org/protocol/muc#user'><item role='participant' affiliation='none'/></x></presence><presence from='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org/pirzde9srwh9d5bup7k1hcm' to='pirzde9srwh9d5bup7k1hcm@itemsxmpp.dev.sui.li'><x ImageUrl='{image.item.nine3q}PirateFlag/image.png' RezableAspect='True' Container='d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org' Label='PirateFlag' Width='43' AnimationsUrl='{image.item.nine3q}PirateFlag/animations.xml' xmlns='vp:props' type='item' Height='65' Name='PirateFlag' provider='nine3q' PageClaimAspect='True' RezzedX='352'/><x xmlns='firebat:user:identity' jid='pirzde9srwh9d5bup7k1hcm@itemsxmpp.dev.sui.li' src='http://localhost:5001/Identity/Generated?avatarUrl=%7Bimage.item.nine3q%7DPirateFlag%2Fanimations.xml&amp;nickname=PirateFlag&amp;digest=1139695235&amp;imageUrl=%7Bimage.item.nine3q%7DPirateFlag%2Fimage.png' digest='1139695235'/><x xmlns='firebat:avatar:state'><position x='352'/></x><x xmlns='http://jabber.org/protocol/muc#user'><item role='participant' affiliation='none'/></x></presence>\r\n";

            var nodeStart = 0;
            var nodeEnd = 0;
            var tagName = "";
            var tagAttributes = new Dictionary<string, string>();
            var tagText = "";
            var sax = new Sax();
            sax.NodeStart += (s, e) => { nodeStart++; tagName = e.Name; tagAttributes = e.Attributes; };
            sax.NodeEnd += (s, e) => { nodeEnd++; tagText = e.Text; };
            sax.ParseError += (s, e) => { throw new System.Exception($"line={e.Line} col={e.Column} {e.Message}"); };

            // Act
            sax.Parse(xml);

            // Assert
            //Assert.AreEqual(1, nodeStart);
            //Assert.AreEqual(1, nodeEnd);
            //Assert.AreEqual("tag", tagName);
            //Assert.AreEqual("&text", tagText);
            //Assert.AreEqual(3, tagAttributes.Count);
            //Assert.AreEqual("v1", tagAttributes["a1"]);
            //Assert.AreEqual("&v2\"", tagAttributes["a2"]);
            //Assert.AreEqual("", tagAttributes["a3"]);
        }

    }
}
