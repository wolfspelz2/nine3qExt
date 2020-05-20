using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Statistics;
using Orleans.Providers;
using nine3q.StorageProviders;
using nine3q.Grains;

namespace IntegrationTests
{
    [TestClass]
    public static class GrainClient
    {
        public const string Category = "WithSilo";
        public const string ClusterId = "test";
        public const string ServiceId = "WeblinItems";

        public static ISiloHost SiloHost { get; set; }
        public static IClusterClient GrainFactory { get; set; }

        private static async Task<ISiloHost> StartSilo()
        {
            // define the cluster configuration
            var builder = new SiloHostBuilder()
                .UseLocalhostClustering()

                .Configure<ClusterOptions>(options => {
                    options.ClusterId = ClusterId;
                    options.ServiceId = ServiceId;
                })

                .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)

                //.ConfigureLogging(logging => {
                //    logging.AddConsole();
                //    logging.SetMinimumLevel(LogLevel.Error);
                //})

                .AddSimpleMessageStreamProvider("SMSProvider", options => {
                    options.FireAndForgetDelivery = true;
                })

                .AddMemoryGrainStorage("PubSubStore")

                .AddMemoryGrainStorage(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME)

                .AddJsonFileStorage(
                    name: JsonFileStorage.StorageProviderName,
                    configureOptions: options => {
                        options.RootDirectory = @"C:\Heiner\github-nine3q\Backend\Test\JsonFileStorage";
                    })

                .AddInventoryFileStorage(
                    name: InventoryFileStorage.StorageProviderName,
                    configureOptions: options => {
                        options.RootDirectory = @"C:\Heiner\github-nine3q\Backend\Test\InventoryFileStorage";
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
                    options.ClusterId = ClusterId;
                    options.ServiceId = ServiceId;
                })
                //.ConfigureLogging(logging => { logging.AddConsole(); logging.SetMinimumLevel(LogLevel.Error); })
                .AddSimpleMessageStreamProvider("SMSProvider")
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
