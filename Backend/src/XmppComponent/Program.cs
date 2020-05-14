using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Orleans.Configuration;
using Orleans.Hosting;

namespace XmppComponent
{
    public class Program
    {
        const int initializeAttemptsBeforeFailing = 5;
        private static int attempt = 0;

        static int Main(string[] args)
        {
            Log.LogLevel = Log.Level.Verbose;
            Log.LogHandler = (level, context, message) => { Console.WriteLine($"{Log.LevelFromString(level.ToString())} {context} {message}"); };

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
            Log.Info("Client successfully connect to silo host");
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

        private static string _componentHost = "items.xmpp.dev.sui.li";
        private static int _port = 5555;
        private static string _sharedSecret = "28756a7ff5dce";

        private static async Task DoClientWork(IClusterClient client)
        {
            var cmdHandler = new CommandHandler(_componentHost, client);
            var conn = new Connection(_componentHost, _port, _sharedSecret, async cmd => { await cmdHandler.HandleCommand(cmd); });
            cmdHandler.Connection = conn;
            await conn.Start();
        }
    }
}