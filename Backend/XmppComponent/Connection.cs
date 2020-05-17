using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XmppComponent
{
    public partial class Connection: IDisposable
    {
        private static NetworkStream _networkStream;

        private static string _host;
        private static int _port;
        private static string _secret;
        private readonly Action<Command> _commandHandler;

        public Connection(string host, int port, string secret, Action<Command> commandHandler)
        {
            _host = host;
            _port = port;
            _secret = secret;
            _commandHandler = commandHandler;
        }

        public async Task Start()
        {
            using (var tcpClient = new TcpClient()) {
                Log.Info("Connecting to server");
                await tcpClient.ConnectAsync(_host, _port);
                Log.Info("Connected to server");

                _networkStream = tcpClient.GetStream();
                {
                    Send($"<stream:stream xmlns='jabber:component:accept' xmlns:stream='http://etherx.jabber.org/streams' to='items.xmpp.dev.sui.li'>");

                    using (var streamReader = new StreamReader(_networkStream))
                    using (var xmlReader = XmlReader.Create(streamReader)) {
                        while (xmlReader.Read()) {
                            switch (xmlReader.NodeType) {
                                case XmlNodeType.XmlDeclaration:
                                Log.Info($"<- {xmlReader.NodeType.ToString()}");
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
                            }
                        }
                    }


                }
            }
        }

        private void OnXmppComponentConnectionStarted()
        {
            Send($"<presence to='item1@{_host}' from='item1@{_host}/backend' />");

            //Send(@$"<presence to='ef1b96243dd54a4f245896a38bcdfb8fdf67b33b@muc4.virtual-presence.org/item1' from='item1@{_host}/backend'>
            //          <x xmlns='http://jabber.org/protocol/muc'>
            //            <history seconds='0' maxchars='0' maxstanzas='0'/>
            //          </x>
            //       </presence>");
        }

        private void OnXmppComponentConnectionStopped()
        {
        }

        private void OnPresence(XmlReader xmlReader)
        {
            var from = xmlReader.GetAttribute("from");
            Log.Verbose($"<-     from={from}");
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
            var cmd = new Command();
            var from = xmlReader.GetAttribute("from");
            var type = xmlReader.GetAttribute("xx");
            Log.Verbose($"<-     from={from}");
            var nodeReader = xmlReader.ReadSubtree();
            while (nodeReader.Read()) {
                switch (nodeReader.NodeType) {
                    case XmlNodeType.Element:
                    if (nodeReader.Depth == 1 && nodeReader.Name == "x" && nodeReader.GetAttribute("xmlns") == "vp:cmd") {
                        nodeReader.MoveToFirstAttribute();
                        var cnt = nodeReader.AttributeCount;
                        while (cnt > 0) {
                            cmd[nodeReader.Name] = nodeReader.Value;
                            nodeReader.MoveToNextAttribute();
                            cnt--;
                        }
                    }
                    break;
                }
            }

            cmd.Select(pair => $"{pair.Key}={pair.Value}").ToList().ForEach(line => Log.Verbose($"<-     {line}"));

            if (cmd.Count > 0) {
                cmd["xmppFrom"] = from ?? "";
                cmd["xmppStanza"] = "message";
                cmd["xmppType"] = type ?? "";
                _commandHandler?.Invoke(cmd);
            }
        }

        public void Send(string text)
        {
            Log.Verbose($"-> {text}");
            var bytes = Encoding.UTF8.GetBytes(text);
            _networkStream.WriteAsync(bytes, 0, bytes.Length).PerformAsyncTaskWithoutAwait(t => Log.Error(t.Exception));
        }


        static string SHA1(string input)
        {
#pragma warning disable CA5350 // Do Not Use Weak Cryptographic Algorithms
            using (var sha1 = new SHA1Managed()) {
#pragma warning restore CA5350 // Do Not Use Weak Cryptographic Algorithms
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                return string.Concat(hash.Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }

        public void Dispose()
        {
            _networkStream?.Dispose();
        }
    }
}
