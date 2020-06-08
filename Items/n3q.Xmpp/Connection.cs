using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using n3q.Tools;

namespace XmppComponent
{
    public class Connection : IDisposable
    {
        private static NetworkStream _networkStream;

        private readonly string _host;
        private readonly string _componentDomain;
        private readonly int _port;
        private readonly string _secret;
        private readonly Action<Connection> _connectionStartHandler;
        private readonly Action<XmppMessage> _xmppMessageHandler;
        private readonly Action<XmppPresence> _xmppPresenceHandler;
        private readonly Action<Connection> _connectionCloseHandler;

        public Connection(string host, string componentDomain, int port, string secret, Action<Connection> connectionStartHandler, Action<XmppMessage> xmppMessageHandler, Action<XmppPresence> xmppPresenceHandler, Action<Connection> connectionCloseHandler)
        {
            _host = host;
            _componentDomain = componentDomain;
            _port = port;
            _secret = secret;
            _connectionStartHandler = connectionStartHandler;
            _xmppMessageHandler = xmppMessageHandler;
            _xmppPresenceHandler = xmppPresenceHandler;
            _connectionCloseHandler = connectionCloseHandler;
        }

        public async Task Run()
        {
            using var tcpClient = new TcpClient();

            Log.Info($"Connecting to server {_host}:{_port}");
            await tcpClient.ConnectAsync(_host, _port);
            Log.Info("Connected");

            _networkStream = tcpClient.GetStream();
            {
                Send($"<stream:stream xmlns='jabber:component:accept' xmlns:stream='http://etherx.jabber.org/streams' to='{WebUtility.HtmlEncode(_componentDomain)}'>");

                using var streamReader = new StreamReader(_networkStream);
                using var xmlReader = XmlReader.Create(streamReader);
                while (xmlReader.Read()) {
                    switch (xmlReader.NodeType) {
                        case XmlNodeType.XmlDeclaration:
                            Log.Verbose($"<- {xmlReader.NodeType.ToString()}");
                            break;

                        case XmlNodeType.Element:
                            switch (xmlReader.Name) {
                                case "stream:stream": {
                                    Log.Verbose($"<- {new String(' ', xmlReader.Depth * 2)}{xmlReader.Name}");
                                    var id = xmlReader.GetAttribute("id");
                                    var data = id + _secret;
                                    var sha1 = SHA1(data);
                                    Send($"<handshake>{sha1}</handshake>");
                                }
                                break;

                                case "handshake": {
                                    Log.Verbose($"<- {new String(' ', xmlReader.Depth * 2)}{xmlReader.Name}");
                                    OnXmppComponentConnectionStarted();
                                }
                                break;

                                case "presence": {
                                    if (xmlReader.Depth == 1) {
                                        Log.Verbose($"<- {new String(' ', xmlReader.Depth * 2)}{xmlReader.Name}");
                                        OnPresence(xmlReader);
                                    } else {
                                        Log.Verbose($"<- {new String(' ', xmlReader.Depth * 2)}{xmlReader.Name}");
                                    }
                                }
                                break;

                                case "message": {
                                    if (xmlReader.Depth == 1) {
                                        Log.Verbose($"<- {new String(' ', xmlReader.Depth * 2)}{xmlReader.Name}");
                                        OnMessage(xmlReader);
                                    } else {
                                        Log.Verbose($"<- {new String(' ', xmlReader.Depth * 2)}{xmlReader.Name}");
                                    }
                                }
                                break;

                                default: {
                                    if (xmlReader.Depth == 1) {
                                        Log.Verbose($"<- {new String(' ', xmlReader.Depth * 2)}{xmlReader.Name}");
                                    } else {
                                        Log.Verbose($"<- {new String(' ', xmlReader.Depth * 2)}{xmlReader.Name}");
                                    }
                                }
                                break;
                            }
                            break;

                            //case XmlNodeType.EndElement:
                            //    Log.Info($"<- {xmlReader.NodeType.ToString()}");
                            //    if (xmlReader.Depth == 1) {
                            //    }
                            //    break;

                            //default:
                            //    Log.Info($"<- {xmlReader.NodeType.ToString()}");
                            //    break;
                    }
                }


            }
            OnXmppComponentConnectionStopped();
        }

        private void OnXmppComponentConnectionStarted()
        {
            //Send($"<presence to='{_componentDomain}' from='item1@{_componentDomain}' />");
            _connectionStartHandler?.Invoke(this);
        }

        private void OnXmppComponentConnectionStopped()
        {
            _connectionCloseHandler?.Invoke(this);
        }

        /*
            <presence to='xmpp.dev.sui.li/nick'>
              <x xmlns='vp:props'
                nickame="yyyyyyy"
		        avatar="xxxxxxx"
	          />
            </presence>

        -->  <message to='items.xmpp.dev.sui.li' from="user-12345@users.virtual-presence.org">
        -->    <x xmlns='vp:cmd'
        -->      method="rez"
		-->      user="user-12345@users.virtual-presence.org"
		-->      room="ef1b96243dd54a4f245896a38bcdfb8fdf67b33b@muc4.virtual-presence.org"
		-->      destination="http://www.mypage.com/index.html"
		-->      x="345"
	    -->    />
        -->  </message>


            <message to='items.xmpp.dev.sui.li'>
              <x xmlns='vp:json'>
              {
		        method: "rez",
		        user: "user-12345@users.virtual-presence.org",
		        room: "ef1b96243dd54a4f245896a38bcdfb8fdf67b33b@muc4.virtual-presence.org",
		        destination: "http://www.mypage.com/index.html",
		        x: 345
              }
	        </x>
            </message>


            <message to='items.xmpp.dev.sui.li'>
              <x xmlns='vp:yaml'>
method: rez
user: user-12345@users.virtual-presence.org
room: ef1b96243dd54a4f245896a38bcdfb8fdf67b33b@muc4.virtual-presence.org
destination: http://www.mypage.com/index.html
x: 345
	        </x>
            </message>


            <message to='items.xmpp.dev.sui.li'>
              <x xmlns='vp:srpc'>
method=rez
user=user-12345@users.virtual-presence.org
room=ef1b96243dd54a4f245896a38bcdfb8fdf67b33b@muc4.virtual-presence.org
destination=http=//www.mypage.com/index.html
x=345
	        </x>
            </message>
        */
        private void OnMessage(XmlReader xmlReader)
        {
            var message = new XmppMessage {
                //MessageType = (xmlReader.GetAttribute("type") ?? "normal") == "groupchat" ? XmppMessageType.Groupchat : XmppMessageType.Normal,
                MessageType = xmlReader.GetAttribute("type") switch
                {
                    "normal" => XmppMessageType.Normal,
                    "groupchat" => XmppMessageType.Groupchat,
                    "chat" => XmppMessageType.PrivateChat,
                    _ => XmppMessageType.Normal,
                },
                From = xmlReader.GetAttribute("from") ?? "",
                To = xmlReader.GetAttribute("to") ?? "",
                Id = xmlReader.GetAttribute("id") ?? "",
            };
            Log.Verbose($"<-     from={message.From}");

            var nodeReader = xmlReader.ReadSubtree();
            while (nodeReader.Read()) {
                switch (nodeReader.NodeType) {
                    case XmlNodeType.Element:
                        if (nodeReader.Depth == 1 && nodeReader.Name == "x" && nodeReader.GetAttribute("xmlns") == "vp:cmd") {
                            nodeReader.MoveToFirstAttribute();
                            var cnt = nodeReader.AttributeCount;
                            while (cnt > 0) {
                                message.Cmd[nodeReader.Name] = nodeReader.Value;
                                nodeReader.MoveToNextAttribute();
                                cnt--;
                            }
                        }
                        break;
                }
            }

            if (message.Cmd.Count > 0) {
                Log.Verbose($"<-     from={message.From}");
                message.Cmd.Select(pair => $"{pair.Key}={pair.Value}").ToList().ForEach(line => Log.Verbose($"<-     {line}"));
                _xmppMessageHandler?.Invoke(message);
            }
        }

        /*
        <presence type="error" to="random-id-hgf5767tigbjhu8ozljnk-09@items.xmpp.dev.sui.li" from="berlin-meetup@conference.conversations.im/random-id-hgf5767tigbjhu8ozljnk-09">
            <error type="cancel" by="items.xmpp.dev.sui.li">
                <remote-server-not-found xmlns="urn:ietf:params:xml:ns:xmpp-stanzas"/>
                <text xmlns="urn:ietf:params:xml:ns:xmpp-stanzas">
                    Server-to-server connection failed: dialback authentication failed
                </text>
            </error>
        </presence>
        */
        private void OnPresence(XmlReader xmlReader)
        {
            var presence = new XmppPresence {
                PresenceType = xmlReader.GetAttribute("type") switch
                {
                    "unavailable" => XmppPresenceType.Unavailable,
                    "error" => XmppPresenceType.Error,
                    _ => XmppPresenceType.Available,
                },
                From = xmlReader.GetAttribute("from") ?? "",
                To = xmlReader.GetAttribute("to") ?? "",
            };
            Log.Verbose($"<- from={presence.From} to={presence.To}");

            Don.t = () => {
                var nodeReader = xmlReader.ReadSubtree();
                while (nodeReader.Read()) {
                    switch (nodeReader.NodeType) {
                        case XmlNodeType.Element:
                            if (nodeReader.Depth == 1 && nodeReader.Name == "x" && nodeReader.GetAttribute("xmlns") == "vp:props") {
                                nodeReader.MoveToFirstAttribute();
                                var cnt = nodeReader.AttributeCount;
                                while (cnt > 0) {
                                    presence.Props[nodeReader.Name] = nodeReader.Value;
                                    nodeReader.MoveToNextAttribute();
                                    cnt--;
                                }
                            }
                            break;
                    }
                }
            };

            Log.Verbose($"<-     from={presence.From}");
            presence.Props.Select(pair => $"{pair.Key}={pair.Value}").ToList().ForEach(line => Log.Verbose($"<-     {line}"));
            _xmppPresenceHandler?.Invoke(presence);
        }

        public void Send(string text)
        {
            Log.Verbose($"-> {text}");
            var bytes = Encoding.UTF8.GetBytes(text);
            _networkStream.WriteAsync(bytes, 0, bytes.Length).PerformAsyncTaskWithoutAwait(t => Log.Error(t.Exception));
        }


        static string SHA1(string input)
        {
            using var sha1 = new SHA1Managed();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Concat(hash.Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
        }

        public void Dispose()
        {
            _networkStream?.Dispose();
        }
    }
}
