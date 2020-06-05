﻿using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Statistics;
using Orleans.Providers;
using n3q.Common;
using n3q.Grains;
using n3q.StorageProviders;
using System.Threading;

namespace LocalSilo
{
    public static class LocalSiloProgram
    {
        //        public static int Main(string[] args)
        public static int Main()
        {
            return RunMainAsync().Result;
        }

        private static async Task<int> RunMainAsync()
        {
            try {
                var host = await StartSilo();
                Console.WriteLine("Press Enter to terminate...");
                Console.ReadLine();
                new AutoResetEvent(false).WaitOne();

                await host.StopAsync();

                return 0;
            } catch (Exception ex) {
                Console.WriteLine(ex);
                return 1;
            }
        }

        private static async Task<ISiloHost> StartSilo()
        {
            // define the cluster configuration
            var builder = new SiloHostBuilder()
                .UseLocalhostClustering()

                .Configure<ClusterOptions>(options => {
                    options.ClusterId = Cluster.DevClusterId;
                    options.ServiceId = Cluster.ServiceId;
                })

                .Configure<EndpointOptions>(options => {
                    options.AdvertisedIPAddress = IPAddress.Loopback;
                    options.GatewayListeningEndpoint = new IPEndPoint(IPAddress.Any, 30000);
                    options.SiloListeningEndpoint = new IPEndPoint(IPAddress.Any, 11111);
                })

                .ConfigureLogging(logging => {
                    logging.AddConsole();

                    //logging.SetMinimumLevel(LogLevel.Error);
                    //logging.SetMinimumLevel(LogLevel.Warning);

                    logging.AddFilter((provider, category, logLevel) => {
                        if (category.Contains("Orleans")) {
                            if (category.Contains(" Orleans.Hosting.SiloHostedService")) {
                                if (logLevel >= LogLevel.Information) {
                                    return true;
                                }
                            } else {
                                if (logLevel >= LogLevel.Error) {
                                    return true;
                                }
                            }
                        } else if (logLevel >= LogLevel.Information) {
                            return true;
                        }
                        return false;
                    });

                })

                .AddSimpleMessageStreamProvider(ItemService.StreamProvider, options => {
                    options.FireAndForgetDelivery = true;
                })


                //.AddMemoryGrainStorage(Cluster.MemoryGrainStorageProviderName)
                .AddJsonFileStorage(
                    name: Cluster.MemoryGrainStorageProviderName,
                    configureOptions: options => {
                        options.RootDirectory = Cluster.MemoryGrainJsonFileStorageRoot;
                    })


                .AddMemoryGrainStorage(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME)

                .AddJsonFileStorage(
                    name: JsonFileStorage.StorageProviderName,
                    configureOptions: options => {
                        options.RootDirectory = ItemService.JsonFileStorageRoot;
                    })

                .UsePerfCounterEnvironmentStatistics()

                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(TestStringGrain).Assembly).WithReferences())
                ;

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}
