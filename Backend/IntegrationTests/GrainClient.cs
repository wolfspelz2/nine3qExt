using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace IntegrationTests
{
    [TestClass]
    public static class GrainClient
    {
        public const string Category = "WithSilo";

        public static IClusterClient GrainFactory { get; set; }

        private static async Task<IClusterClient> Start()
        {
            var client = new ClientBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options => {
                    options.ClusterId = "dev";
                    options.ServiceId = "Sample";
                })
                //.ConfigureLogging(logging => { logging.AddConsole(); logging.SetMinimumLevel(LogLevel.Error); })
                .AddSimpleMessageStreamProvider("SMSProvider")
                .Build();

            await client.Connect();
            return client;
        }

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            GrainFactory = IntegrationTests.GrainClient.Start().Result;
        }
    }
}
