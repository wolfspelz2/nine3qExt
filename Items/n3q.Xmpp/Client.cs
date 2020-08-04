using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace n3q.Xmpp
{
    public class Client
    {
        ConnectedCallback _onConnected = null;
        DataCallback _onData = null;
        DisconnectedCallback _onDisconnected = null;

        Socket _socket = null;
        const int BUFFER_SIZE = 10240;
        readonly byte[] _bytes = new byte[BUFFER_SIZE];
        readonly int _size = BUFFER_SIZE;
        bool _isReceiving = false;

        internal delegate void ConnectedCallback();
        internal delegate void DataCallback(byte[] data);
        internal delegate void DisconnectedCallback();

        internal async Task ConnectAsync(string host, int port, ConnectedCallback onConnected, DataCallback onData, DisconnectedCallback onDisconnected)
        {
            _onConnected = onConnected;
            _onData = onData;
            _onDisconnected = onDisconnected;

            Log.Info("Connecting...");

            var newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var ihe = await Dns.GetHostEntryAsync(host);
            var iep = new IPEndPoint(ihe.AddressList[0], port);
            newSocket.BeginConnect(iep, new AsyncCallback(OnConnected), newSocket);
        }

        internal void Disconnect()
        {
            if (_socket != null) {
                var socket = _socket;
                _socket = null;
                socket.Close();

                Log.Info("Disconnected");
            }
        }

        internal void Send(string sText)
        {
            byte[] message = Encoding.UTF8.GetBytes(sText);
            lock (this) {
                //Log.Info("OUT " + sText);
                _socket.BeginSend(message, 0, message.Length, SocketFlags.None, new AsyncCallback(OnSent), _socket);
            }
        }

        void Receive()
        {
            lock (this) {
                if (!_isReceiving) {
                    if (_socket != null) {
                        _isReceiving = true;
                        _socket.BeginReceive(_bytes, 0, _size, SocketFlags.None, new AsyncCallback(OnReceived), _socket);
                    }
                }
            }
        }

        void OnConnected(IAsyncResult iar)
        {
            _socket = (Socket)iar.AsyncState;

            try {
                _socket.EndConnect(iar);

                Log.Info("Connected to: " + _socket.RemoteEndPoint.ToString());

                _onConnected?.Invoke();

                Receive();
            } catch (SocketException ex) {
                Log.Error("Error connecting: " + ex.Message);

                _onDisconnected?.Invoke();
            }
        }

        void OnReceived(IAsyncResult iar)
        {
            lock (this) {
                if (_socket == null) {
                    OnDisconnected();
                } else {
                    _isReceiving = false;
                    int bytesRead = 0;
                    try {
                        bytesRead = _socket.EndReceive(iar);
                    } catch { }

                    if (bytesRead == 0) {
                        OnDisconnected();
                    } else {
                        //string data = Encoding.UTF8.GetString(_bytes, 0, bytesRead);
                        //Log("IN " + data);

                        _onData?.Invoke(_bytes);

                        Receive();
                    }
                }
            }
        }

        void OnSent(IAsyncResult iar)
        {
            lock (this) {
                var remoteSocket = (Socket)iar.AsyncState;
                int sent = remoteSocket.EndSend(iar);
            }

            Receive();
        }

        void OnDisconnected()
        {
            Disconnect();

            _onDisconnected?.Invoke();
        }
    }
}
