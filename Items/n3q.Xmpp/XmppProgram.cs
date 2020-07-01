using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Orleans.Configuration;
using Orleans.Hosting;
using n3q.Common;
using n3q.Tools;

namespace n3q.Xmpp
{
    public class XmppProgram
    {
        private static int _attempt = 0;

        static readonly XmppConfigDefinition Config = new XmppConfigDefinition();
        static Controller _controller;

        static int Main(string[] args)
        {
            Log.LogLevel = Log.Level.Info;
            Log.LogHandler = (lvl, ctx, msg) => { Console.WriteLine($"{lvl} {ctx} {msg}"); };

            ConfigSharp.Log.LogLevel = ConfigSharp.Log.Level.Info;
            ConfigSharp.Log.LogHandler = (lvl, ctx, msg) => { Log.DoLog(Log.LevelFromString(lvl.ToString()), ctx, msg); };
            Config.ConfigFile = nameof(XmppConfig) + ".cs";
            Config.ParseCommandline(args);
            Config.Include(Config.ConfigFile);
            Console.WriteLine($"RunMode={Config.Build} ConfigSequence={Config.ConfigSequence}");

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
            _attempt = 0;

            var builder = new ClientBuilder();

            if (Config.LocalhostClustering) {
                builder.UseLocalhostClustering();
            } else {
                builder.UseAzureStorageClustering(options => options.ConnectionString = Config.ClusteringAzureTableConnectionString);
            }

            builder.Configure<ClusterOptions>(options => {
                options.ClusterId = Config.ClusterId;
                options.ServiceId = Cluster.ServiceId;
            });
            builder.AddClusterConnectionLostHandler(OnClusterConnectionDown);
            builder.AddGatewayCountChangedHandler(OnClusterGatewayCountChanged);
            builder.ConfigureLogging(logging => { logging.AddConsole(); logging.SetMinimumLevel(LogLevel.Error); });
            builder.AddSimpleMessageStreamProvider(ItemService.StreamProvider);

            IClusterClient client = builder.Build();

            await client.Connect(RetryFilter);
            Log.Info("Client connected to silo host");

            return client;
        }

        private static void OnClusterGatewayCountChanged(object sender, GatewayCountChangedEventArgs e)
        {
            Console.WriteLine($"OnClusterGatewayCountChanged: {e.PreviousNumberOfConnectedGateways} -> {e.NumberOfConnectedGateways}");
            if (e.PreviousNumberOfConnectedGateways == 0 && e.NumberOfConnectedGateways > 0) {
                _controller?.OnClusterReconnect();
            }
        }

        private static void OnClusterConnectionDown(object sender, EventArgs e)
        {
            Console.WriteLine($"OnClusterConnectionDown");
            _controller?.OnClusterDisconnect();
        }

        private static async Task<bool> RetryFilter(Exception exception)
        {
            _attempt++;
            Console.WriteLine($"Cluster client attempt {_attempt} failed to connect: {exception.GetType().Name} {exception.Message}");

            await Task.Delay(TimeSpan.FromSeconds(Config.ClusterConnectSecondsBetweenRetries));
            return true;
        }

        private static async Task DoClientWork(IClusterClient client)
        {
            _controller = new Controller(client, Config);
            await _controller.Start();

            if (Config.Build == XmppConfigDefinition.BuildConfiguration.Release) {

                Console.WriteLine("Press CTRL-C to terminate...");
                new AutoResetEvent(false).WaitOne();

            } else {

                Console.WriteLine("Press Enter to terminate...");
                var line = "";
                do {
                    line = Console.ReadLine();
                    _controller.Send(line);
                } while (Has.Value(line) && line != "q");

            }

            await _controller.Shutdown();

            await Task.CompletedTask;
        }
    }
}