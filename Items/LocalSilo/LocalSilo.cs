using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Statistics;
using Orleans.Providers;
using n3q.Grains;
using n3q.StorageProviders;

namespace LocalSilo
{
    public static class LocalSilo
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
                    options.ClusterId = "dev";
                    options.ServiceId = "WeblinItems";
                })

                .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)

                .ConfigureLogging(logging => {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Error);
                })

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

                .UsePerfCounterEnvironmentStatistics()

                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(TestStringGrain).Assembly).WithReferences())
                ;

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}
