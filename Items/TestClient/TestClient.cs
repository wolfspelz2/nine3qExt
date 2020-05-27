﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Streams;
using n3q.GrainInterfaces;
using n3q.Common;

namespace TestClient
{
    //public class StringCacheObserver : IAsyncObserver<string>
    //{
    //}

    public class TestClient
    {
        const int InitializeAttemptsBeforeFailing = 5;
        private static int _attempt = 0;

#pragma warning disable IDE0060 // Remove unused parameter
        static int Main(string[] args)
#pragma warning restore IDE0060 // Remove unused parameter
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
            _attempt = 0;
            IClusterClient client;
            client = new ClientBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options => {
                    options.ClusterId = Cluster.DevClusterId;
                    options.ServiceId = Cluster.ServiceId;
                })
                .ConfigureLogging(logging => { logging.AddConsole(); logging.SetMinimumLevel(LogLevel.Error); })
                .AddSimpleMessageStreamProvider(Cluster.SimpleMessageStreamProviderName)
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
            Console.WriteLine("test1");
            Console.WriteLine("[set|get|inc|sub|unsub] key (value)");

            var subscriptionHandles = new Dictionary<string, StreamSubscriptionHandle<string>>();

            while (true) {
                var line = Console.ReadLine();

                if (string.IsNullOrEmpty(line)) {
                    Console.WriteLine($"Terminating");
                    return;
                }

                var parts = line.Split(" ", 3, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length < 1) {
                    Console.WriteLine($"Terminating");
                    return;
                }

                var action = parts[0];

                if (action == "quit" || action == "q" || parts.Length < 2) {
                    Console.WriteLine($"Terminating");
                    return;
                }

                var key = parts[1];
                var grain = client.GetGrain<ITestString>(key);

                switch (action) {
                    case "set": {
                        var value = parts[2];
                        await grain.Set(value);
                        Console.WriteLine($"{key} <- {value}");
                    }
                    break;

                    case "get": {
                        var value = await grain.Get();
                        Console.WriteLine($"{key} -> {value}");
                    }
                    break;

                    case "inc": {
                        var value = await grain.Get();
                        long.TryParse(value, out long num);
                        num++;
                        await grain.Set(num.ToString());
                        Console.WriteLine($"{key} <- {num}");
                    }
                    break;

                    case "sub": {
                        var streamProvider = client.GetStreamProvider(TestStringStream.Provider);
                        var stream = streamProvider.GetStream<string>(await grain.GetStreamId(), TestStringStream.Namespace);
                        if (subscriptionHandles.ContainsKey(key)) {
                            await subscriptionHandles[key].UnsubscribeAsync();
                            subscriptionHandles.Remove(key);
                        }
                        subscriptionHandles[key] = await stream.SubscribeAsync<string>((data, token) => {
                            Console.WriteLine($"{key} >> {data}");
                            return Task.CompletedTask;
                        });
                        Console.WriteLine($"{key} >> ");
                    }
                    break;

                    case "unsub": {
                        if (subscriptionHandles.ContainsKey(key)) {
                            await subscriptionHandles[key].UnsubscribeAsync();
                            subscriptionHandles.Remove(key);
                            Console.WriteLine($"{key} >>| ");
                        }
                    }
                    break;

                }
            }
        }
    }
}