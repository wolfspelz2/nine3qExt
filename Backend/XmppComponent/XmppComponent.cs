using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Orleans.Configuration;
using Orleans.Hosting;
using nine3q.GrainInterfaces;

namespace XmppComponent
{
    public class XmppComponent
    {
        const int initializeAttemptsBeforeFailing = 5;
        private static int attempt = 0;

        static int Main(string[] args)
        {
            Log.LogLevel = Log.Level.Info;
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
                    options.ServiceId = "WeblinItems";
                })
                .ConfigureLogging(logging => { logging.AddConsole(); logging.SetMinimumLevel(LogLevel.Error); })
                .AddSimpleMessageStreamProvider("SMSProvider")
                .Build();

            await client.Connect(RetryFilter);
            Log.Info("Client connected to silo host", nameof(StartClientWithRetries));

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

        private const string _componentHost = "items.xmpp.dev.sui.li";
        private const string _componentDomain = "items.xmpp.dev.sui.li";
        private const int _componentPort = 5555;
        private const string _componentSecret = "28756a7ff5dce";

        private static async Task DoClientWork(IClusterClient client)
        {

            var controller = new Controller(client,
                _componentHost,
                _componentDomain,
                _componentPort,
                _componentSecret
            );
            controller.Start();

            Console.WriteLine("Press Enter to terminate...");
            var line = "";
            while (line != "q") {
                line = Console.ReadLine();
                controller.Send(line);
            }

            await Task.CompletedTask;
        }
    }
}