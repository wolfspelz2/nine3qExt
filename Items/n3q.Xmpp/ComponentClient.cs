using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using n3q.Tools;

namespace n3q.Xmpp
{
    public class ComponentClient
    {
        public delegate void ConnectedAndLoggedInCallback();
        public delegate void DisconnectedCallback();

        readonly string _host;
        readonly string _componentDomain;
        readonly int _port;
        readonly string _secret;

        readonly ConnectedAndLoggedInCallback _onConnected;
        readonly DisconnectedCallback _onDisconnected;
        readonly Action<XmppPresence> _onXmppPresence;
        readonly Action<XmppMessage> _onXmppMessage;

        Client _client;
        Sax _sax;

        bool _loggedIn = false;
        XmppMessage _currentMessage;

        public ComponentClient(string host, string componentDomain, int port, string secret, ConnectedAndLoggedInCallback onConnected, DisconnectedCallback onDisconnected, Action<XmppPresence> onXmppPresence, Action<XmppMessage> onXmppMessage)
        {
            _host = host;
            _componentDomain = componentDomain;
            _port = port;
            _secret = secret;

            _onConnected = onConnected;
            _onDisconnected = onDisconnected;
            _onXmppPresence = onXmppPresence;
            _onXmppMessage = onXmppMessage;
        }

        public async Task Connect()
        {
            _client = new Client();

            _sax = new Sax();
            _sax.StartElement += Sax_StartElement;
            _sax.EndElement += Sax_EndElement;
            _sax.ParseError += Sax_ParseError;

            await _client?.Connect(_host, _port, Client_OnConnected, Client_OnData, Client_OnDisconnected);
        }

        public void Disconnect()
        {
            _client?.Disconnect();
        }

        public void Send(string text)
        {
            _client?.Send(text);
        }

        void Client_OnConnected()
        {
            Log.Info("");
            Send($"<stream:stream xmlns='jabber:component:accept' xmlns:stream='http://etherx.jabber.org/streams' to='{WebUtility.HtmlEncode(_componentDomain)}'>");
        }

        void Client_OnData(byte[] data)
        {
            Log.Info($"{data.Length} bytes");

            _sax.Parse(data);
        }

        void Client_OnDisconnected()
        {
            Log.Info("");
            _client = null;
            _sax = null;
            _onDisconnected?.Invoke();
        }

        private void Sax_StartElement(object sender, Sax.StartElementArgs e)
        {
            Log.Info($"{e.Depth} {e.Name}");

            if (!_loggedIn && e.Name == "stream:stream") {
                SendHandshake(e.Attributes);
            } else if (_loggedIn && e.Depth == 1) {
                BeginStanza(e.Name, e.Attributes);
            } else if (_loggedIn && e.Depth == 2) {
                StanzaChild(e.Name, e.Attributes);
            }
        }

        void Sax_EndElement(object sender, Sax.EndElementArgs e)
        {
            Log.Info($"{e.Name}");

            if (!_loggedIn && e.Name == "handshake") {
                _loggedIn = true;
                _onConnected?.Invoke();
            } else if (_loggedIn && e.Depth == 1) {
                EndStanza(e.Name);
            }
        }

        void Sax_ParseError(object sender, Sax.ParseErrorArgs e)
        {
            Log.Error($"line={e.Line} col={e.Column} [{e.Message}] around: [{e.Vicinity}]");
            Disconnect();
        }

        void SendHandshake(Sax.AttributeSet attributes)
        {
            var id = attributes["id"];
            var data = id + _secret;
            var sha1 = Crypto.SHA1Hex(data);
            Send($"<handshake>{sha1}</handshake>");
        }

        void BeginStanza(string name, Sax.AttributeSet attributes)
        {
            _currentMessage = null;

            switch (name) {
                case "presence":
                    OnBeginPresence(attributes);
                    break;

                case "message":
                    OnBeginMessage(attributes);
                    break;
            }
        }

        void EndStanza(string name)
        {
            switch (name) {
                case "presence":
                    OnEndPresence();
                    break;

                case "message":
                    OnEndMessage();
                    break;
            }
        }

        void StanzaChild(string name, Sax.AttributeSet attributes)
        {
            if (_currentMessage == null) { return; }
            var message = _currentMessage;

            if (name == "x" && attributes.Get("xmlns") == "vp:cmd") {
                foreach (var kv in attributes) {
                    message.Cmd[kv.Key] = kv.Value;
                }
            }
        }

        void OnBeginPresence(Sax.AttributeSet attributes)
        {
            var presence = new XmppPresence {
                PresenceType = attributes.Get("type") switch
                {
                    "unavailable" => XmppPresenceType.Unavailable,
                    "error" => XmppPresenceType.Error,
                    _ => XmppPresenceType.Available,
                },
                From = attributes.Get("from") ?? "",
                To = attributes.Get("to") ?? "",
            };
            Log.Verbose($"<- from={presence.From} to={presence.To}");
            presence.Props.Select(pair => $"{pair.Key}={pair.Value}").ToList().ForEach(line => Log.Verbose($"<-     {line}"));
            _onXmppPresence?.Invoke(presence);
        }

        private void OnEndPresence()
        {
        }

        private void OnBeginMessage(Sax.AttributeSet attributes)
        {
            var message = new XmppMessage {
                //MessageType = (xmlReader.GetAttribute("type") ?? "normal") == "groupchat" ? XmppMessageType.Groupchat : XmppMessageType.Normal,
                MessageType = attributes.Get("type") switch
                {
                    "normal" => XmppMessageType.Normal,
                    "groupchat" => XmppMessageType.Groupchat,
                    "chat" => XmppMessageType.PrivateChat,
                    _ => XmppMessageType.Normal,
                },
                From = attributes.Get("from") ?? "",
                To = attributes.Get("to") ?? "",
                Id = attributes.Get("id") ?? "",
            };
            Log.Verbose($"<-     from={message.From}");

            _currentMessage = message;
        }

        private void OnEndMessage()
        {
            if (_currentMessage == null) { return; }
            var message = _currentMessage;

            if (message.Cmd.Count > 0) {
                Log.Verbose($"<-     from={message.From}");
                message.Cmd.Select(pair => $"{pair.Key}={pair.Value}").ToList().ForEach(line => Log.Verbose($"<-     {line}"));
                _onXmppMessage?.Invoke(message);
            }
        }

    }
}
