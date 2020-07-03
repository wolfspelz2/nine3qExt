using System.Net;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Statistics;
using Orleans.Providers;
using n3q.Common;
using n3q.Tools;
using n3q.StorageProviders;
using n3q.Grains;
using n3q.Aspects;

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
                    options.ClusterId = "test";
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
                        options.RootDirectory = Cluster.DevelopmentJsonFileStorageRoot;
                    })

                .AddKeyValueFileStorage(
                    name: KeyValueFileStorage.StorageProviderName,
                    configureOptions: options => {
                        options.RootDirectory = Cluster.DevelopmentKeyValueFileStorageRoot;
                    })

                .AddItemAzureTableStorage(
                    name: ItemAzureTableStorage.StorageProviderName,
                    configureOptions: options => {
                        options.TableName = "n3qTest";
                        options.ConnectionString = "UseDevelopmentStorage=true";
                    })

                .AddReflectingAzureTableStorage(
                    name: ReflectingAzureTableStorage.StorageProviderName,
                    configureOptions: options => {
                        options.TableName = "n3qTest";
                        options.ConnectionString = "UseDevelopmentStorage=true";
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
                    options.ClusterId = "test";
                    options.ServiceId = Cluster.ServiceId;
                })
                //.ConfigureLogging(logging => { logging.AddConsole(); logging.SetMinimumLevel(LogLevel.Error); })
                .AddSimpleMessageStreamProvider(ItemService.StreamProvider)
                .Build();

            await client.Connect();
            return client;
        }

        public static string GetRandomItemId(string id = null, [CallerFilePath]string callerFilePath = null, [CallerMemberName]string callerMemberName = null)
        {
            if (!Has.Value(id)) {
                id = "";
            }
            if (Has.Value(callerMemberName)) {
                id = callerMemberName + "-" + id;
            }
            if (Has.Value(callerFilePath)) {
                var guessedCallerTypeName = Path.GetFileNameWithoutExtension(callerFilePath);
                id = guessedCallerTypeName + "-" + id;
            }
            id += RandomString.Get(10);

            return id;
        }

        public static ItemStub GetItemStub(string id)
        {
            return new ItemStub(new OrleansClusterClient(GrainFactory, id));
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
