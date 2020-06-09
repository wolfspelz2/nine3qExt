﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Orleans.Configuration;
using Orleans.Hosting;
using n3q.Common;
using n3q.Tools;

namespace XmppComponent
{
    public class XmppProgram
    {
        const int InitializeAttemptsBeforeFailing = 5;

        private const string ComponentHost = "itemsxmpp.dev.sui.li";
        private const string ComponentDomain = "itemsxmpp.dev.sui.li";
        private const int ComponentPort = 5555;//5280;//5555;
        private const string ComponentSecret = "28756a7ff5dce";

        private static int _attempt = 0;
        private static Controller _controller;

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
            _attempt = 0;
            IClusterClient client;
            client = new ClientBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options => {
                    options.ClusterId = Cluster.DevClusterId;
                    options.ServiceId = Cluster.ServiceId;
                })
                .AddClusterConnectionLostHandler(OnClusterConnectionDown)
                .AddGatewayCountChangedHandler(OnClusterGatewayCountChanged)
                .ConfigureLogging(logging => { logging.AddConsole(); logging.SetMinimumLevel(LogLevel.Error); })
                .AddSimpleMessageStreamProvider(ItemService.StreamProvider)
                .Build();

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
            if (exception.GetType() != typeof(SiloUnavailableException)) {
                Console.WriteLine($"Cluster client failed to connect to cluster with unexpected error.  Exception: {exception}");
                return false;
            }
            _attempt++;
            Console.WriteLine($"Cluster client attempt {_attempt} of {InitializeAttemptsBeforeFailing} failed to connect to cluster.  Exception: {exception}");
            if (_attempt > InitializeAttemptsBeforeFailing) {
                return false;
            }
            await Task.Delay(TimeSpan.FromSeconds(4));
            return true;
        }

        private static async Task DoClientWork(IClusterClient client)
        {
            _controller = new Controller(client,
                ComponentHost,
                ComponentDomain,
                ComponentPort,
                ComponentSecret
            );

            await _controller.Start();

            Console.WriteLine("Press Enter to terminate...");
            var line = "";
            do {
                line = Console.ReadLine();
                _controller.Send(line);
            } while (Has.Value(line) && line != "q");

            await _controller.Shutdown();

            await Task.CompletedTask;
        }
    }
}