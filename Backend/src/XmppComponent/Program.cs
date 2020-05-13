using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Streams;
using Nine3Q.GrainInterfaces;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Security.Cryptography;
using System.Linq;
using System.Net;

//using System.Net.Sockets;
//using System.Net;
//using System.Text;
//using System.Xml;

//using Waher.Networking.XMPP;

//using Matrix;
//using System.Reactive.Linq;
//using Matrix.Xml;
//using DotNetty.Transport.Channels;
//using DotNetty.Transport.Bootstrapping;
//using System.Net;

namespace Nine3Q.Client
{
    public class Program
    {
        const int initializeAttemptsBeforeFailing = 5;
        private static int attempt = 0;

        static int Main(string[] args)
        {
            return RunMainAsync().Result;
        }

        private static async Task<int> RunMainAsync()
        {
            try
            {
                using (var client = await StartClientWithRetries())
                {
                    await DoClientWork(client);
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
                return 1;
            }
        }

        private static async Task<IClusterClient> StartClientWithRetries()
        {
            attempt = 0;
            IClusterClient client;
            client = new ClientBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "Sample";
                })
                .ConfigureLogging(logging => { logging.AddConsole(); logging.SetMinimumLevel(LogLevel.Error); })
                .AddSimpleMessageStreamProvider("SMSProvider")
                .Build();

            await client.Connect(RetryFilter);
            Console.WriteLine("Client successfully connect to silo host");
            return client;
        }

        private static async Task<bool> RetryFilter(Exception exception)
        {
            if (exception.GetType() != typeof(SiloUnavailableException))
            {
                Console.WriteLine($"Cluster client failed to connect to cluster with unexpected error.  Exception: {exception}");
                return false;
            }
            attempt++;
            Console.WriteLine($"Cluster client attempt {attempt} of {initializeAttemptsBeforeFailing} failed to connect to cluster.  Exception: {exception}");
            if (attempt > initializeAttemptsBeforeFailing)
            {
                return false;
            }
            await Task.Delay(TimeSpan.FromSeconds(4));
            return true;
        }

        private static NetworkStream _networkStream;
        private static string _componentHost = "items.xmpp.dev.sui.li";
        private static int _port = 5555;
        private static string _sharedSecret = "28756a7ff5dce";

        private static async Task DoClientWork(IClusterClient client)
        {
            //await Task.CompletedTask;

            using (var tcpClient = new TcpClient())
            {
                Console.WriteLine("Connecting to server");
                await tcpClient.ConnectAsync(_componentHost, _port);
                Console.WriteLine("Connected to server");
                _networkStream = tcpClient.GetStream();
                {
                    {
                        var textToSend = "<stream:stream xmlns='jabber:component:accept' xmlns:stream='http://etherx.jabber.org/streams' to='items.xmpp.dev.sui.li'>";
                        Console.WriteLine($"-> {textToSend}");
                        var bytesToSend = Encoding.UTF8.GetBytes(textToSend);
                        await _networkStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
                    }

                    //var buffer = new byte[4096];
                    //var byteCount = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                    //var received = Encoding.UTF8.GetString(buffer, 0, byteCount);
                    //Console.WriteLine($"<- {received}");

                    using (var streamReader = new StreamReader(_networkStream))
                    using (var xmlReader = XmlReader.Create(streamReader))
                    {
                        while (xmlReader.Read())
                        {
                            switch (xmlReader.NodeType)
                            {
                                case XmlNodeType.XmlDeclaration:
                                    Console.WriteLine($"<- {xmlReader.NodeType.ToString()}");
                                    break;
                                case XmlNodeType.Element:
                                    Console.WriteLine($"<- {xmlReader.NodeType.ToString()} {xmlReader.Name}");

                                    switch (xmlReader.Name)
                                    {
                                        case "stream:stream":
                                            var id = xmlReader.GetAttribute("id");
                                            var data = id + _sharedSecret;
                                            var sha1 = SHA1(data);
                                            await SendStanza($"<handshake>{sha1}</handshake>");
                                            break;

                                        case "handshake":
                                            await OnXmppComponentConnectionStartedAsync();
                                            break;

                                        case "presence":
                                            OnPresence(XElement.ReadFrom(xmlReader));
                                            break;

                                        case "message":
                                            OnMessage(XElement.ReadFrom(xmlReader));
                                            break;
                                    }

                                    break;
                            }
                        }
                    }


                }
            }
        }

        private static async Task OnXmppComponentConnectionStartedAsync()
        {
            await SendStanza($"<presence to='ef1b96243dd54a4f245896a38bcdfb8fdf67b33b@muc4.virtual-presence.org/item1' from='item1@{_componentHost}' />");
        }

        private static void OnXmppComponentConnectionStopped()
        {
        }

        private static void OnPresence(XNode stanzaNode)
        {
            //if (stanzaNode is XElement stanza)
            //{
            //    var from = stanza.Attribute("from").Value;
            //    Console.WriteLine($"-> presence of {from}");
            //}
        }

        private static void OnMessage(XNode stanzaNode)
        {
        }

        // ---

        private static async Task SendStanza(string text)
        {
            Console.WriteLine($"-> {text}");
            var bytes = Encoding.UTF8.GetBytes(text);
            await _networkStream.WriteAsync(bytes, 0, bytes.Length);
        }

        static string SHA1(string input)
        {
            var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }

    }
}
