﻿using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Statistics;
using Orleans.Providers;
using n3q.Common;
using n3q.StorageProviders;
using n3q.Grains;

namespace IntegrationTests
{
    [TestClass]
    public static class GrainClient
    {
        public const string Category = "WithSilo";

        public static ISiloHost SiloHost { get; set; }
        public static IClusterClient GrainFactory { get; set; }

        private static async Task<ISiloHost> StartSilo()
        {
            // define the cluster configuration
            var builder = new SiloHostBuilder()
                .UseLocalhostClustering()

                .Configure<ClusterOptions>(options => {
                    options.ClusterId = Cluster.TestClusterId;
                    options.ServiceId = Cluster.ServiceId;
                })

                .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)

                //.ConfigureLogging(logging => {
                //    logging.AddConsole();
                //    logging.SetMinimumLevel(LogLevel.Error);
                //})

                .AddSimpleMessageStreamProvider(ItemService.StreamProvider, options => {
                    options.FireAndForgetDelivery = true;
                })

                .AddMemoryGrainStorage(Cluster.MemoryGrainStorageProviderName)

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

        private static async Task<IClusterClient> StartClient()
        {
            var client = new ClientBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options => {
                    options.ClusterId = Cluster.DevClusterId;
                    options.ServiceId = Cluster.ServiceId;
                })
                //.ConfigureLogging(logging => { logging.AddConsole(); logging.SetMinimumLevel(LogLevel.Error); })
                .AddSimpleMessageStreamProvider(ItemService.StreamProvider)
                .Build();

            await client.Connect();
            return client;
        }

        [AssemblyInitialize]
        public static async Task AssemblyInit(TestContext context)
        {
            SiloHost = await IntegrationTests.GrainClient.StartSilo();
            GrainFactory = await  IntegrationTests.GrainClient.StartClient();
        }

        [AssemblyCleanup]
        public static async Task AssemblyCleanup()
        {
            await SiloHost.StopAsync();
        }
    }
}