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
            try {
                using (var client = await StartClientWithRetries()) {
                    await DoClientWork(client);
                }

                return 0;
            } catch (Exception e) {
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
                .Configure<ClusterOptions>(options => {
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
            if (exception.GetType() != typeof(SiloUnavailableException)) {
                Console.WriteLine($"Cluster client failed to connect to cluster with unexpected error.  Exception: {exception}");
                return false;
            }
            attempt++;
            Console.WriteLine($"Cluster client attempt {attempt} of {initializeAttemptsBeforeFailing} failed to connect to cluster.  Exception: {exception}");
            if (attempt > initializeAttemptsBeforeFailing) {
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

            using (var tcpClient = new TcpClient()) {
                Console.WriteLine("Connecting to server");
                await tcpClient.ConnectAsync(_componentHost, _port);
                Console.WriteLine("Connected to server");
                _networkStream = tcpClient.GetStream();
                {
                    Send($"<stream:stream xmlns='jabber:component:accept' xmlns:stream='http://etherx.jabber.org/streams' to='items.xmpp.dev.sui.li'>");

                    using (var streamReader = new StreamReader(_networkStream))
                    using (var xmlReader = XmlReader.Create(streamReader)) {
                        while (xmlReader.Read()) {
                            switch (xmlReader.NodeType) {
                                case XmlNodeType.XmlDeclaration:
                                Console.WriteLine($"<- {xmlReader.NodeType.ToString()}");
                                break;

                                case XmlNodeType.Element:
                                switch (xmlReader.Name) {
                                    case "stream:stream": {
                                        Console.WriteLine($"<- {new String(' ', xmlReader.Depth * 2)}{xmlReader.Name}");
                                        var id = xmlReader.GetAttribute("id");
                                        var data = id + _sharedSecret;
                                        var sha1 = SHA1(data);
                                        Send($"<handshake>{sha1}</handshake>");
                                    }
                                    break;

                                    case "handshake": {
                                        Console.WriteLine($"<- {new String(' ', xmlReader.Depth * 2)}{xmlReader.Name}");
                                        OnXmppComponentConnectionStarted();
                                    }
                                    break;

                                    case "presence": {
                                        if (xmlReader.Depth == 1) {
                                            //var x = XNode.ReadFrom(xmlReader);
                                            //Console.WriteLine($"<- {x.ToString()}");
                                            Console.WriteLine($"<- {new String(' ', xmlReader.Depth * 2)}{xmlReader.Name}");
                                            OnPresence(xmlReader);
                                        } else {
                                            Console.WriteLine($"<- {new String(' ', xmlReader.Depth * 2)}{xmlReader.Name}");
                                        }
                                    }
                                    break;

                                    case "message": {
                                        if (xmlReader.Depth == 1) {
                                            //var x = XNode.ReadFrom(xmlReader);
                                            //Console.WriteLine($"<- {x.ToString()}");
                                            Console.WriteLine($"<- {new String(' ', xmlReader.Depth * 2)}{xmlReader.Name}");
                                            OnMessage(xmlReader);
                                        } else {
                                            Console.WriteLine($"<- {new String(' ', xmlReader.Depth * 2)}{xmlReader.Name}");
                                        }
                                    }
                                    break;

                                    default: {
                                        if (xmlReader.Depth == 1) {
                                            //var x = XNode.ReadFrom(xmlReader);
                                            //Console.WriteLine($"<- {x.ToString()}");
                                            Console.WriteLine($"<- {new String(' ', xmlReader.Depth * 2)}{xmlReader.Name}");
                                        } else {
                                            Console.WriteLine($"<- {new String(' ', xmlReader.Depth * 2)}{xmlReader.Name}");
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

        private static void OnXmppComponentConnectionStarted()
        {
            Send($"<presence to='item1@{_componentHost}' from='item1@{_componentHost}/backend' />");

            Send(@$"<presence to='ef1b96243dd54a4f245896a38bcdfb8fdf67b33b@muc4.virtual-presence.org/item1' from='item1@{_componentHost}/backend'>
                      <x xmlns='http://jabber.org/protocol/muc'>
                        <history seconds='0' maxchars='0' maxstanzas='0'/>
                      </x>
                   </presence>");
        }

        private static void OnXmppComponentConnectionStopped()
        {
        }

        private static void OnPresence(XmlReader xmlReader)
        {
            var from = xmlReader.GetAttribute("from");
            Console.WriteLine($"<-     from={from}");
        }

        /*
            <presence to='xmpp.dev.sui.li/nick'>
              <x xmlns='vp:props'
                nickame="yyyyyyy"
		        avatar="xxxxxxx"
	          />
            </presence>

        -->  <message to='items.xmpp.dev.sui.li'>
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
        private static void OnMessage(XmlReader xmlReader)
        {
            var cmd = new Dictionary<string, string>();
            var from = xmlReader.GetAttribute("from");
            Console.WriteLine($"<-     from={from}");
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

            cmd.Select(pair => $"{pair.Key}={pair.Value}").ToList().ForEach(line => Console.WriteLine($"<-     {line}"));

            if (cmd.Count > 0) {
            }
        }

        private static void Send(string text)
        {
            Console.WriteLine($"-> {text}");
            var bytes = Encoding.UTF8.GetBytes(text);
            _networkStream.WriteAsync(bytes, 0, bytes.Length).PerformAsyncTaskWithoutAwait(t => Console.WriteLine(t.Exception));
        }

        static string Serialize(XNode node)
        {
            using StringWriter sw = new StringWriter();
            (node as XElement).Save(sw);
            return sw.ToString();
        }

        static string SHA1(string input)
        {
            var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }

    }

    public static class AsyncUtility
    {
        public static void PerformAsyncTaskWithoutAwait(this Task task, Action<Task> exceptionHandler)
        {
            var dummy = task.ContinueWith(t => exceptionHandler(t), TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}