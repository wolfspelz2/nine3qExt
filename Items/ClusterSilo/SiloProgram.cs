using System;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Statistics;
using Orleans.Providers;
using n3q.Common;
using n3q.Grains;
using n3q.StorageProviders;

namespace ClusterSilo
{
    public static class SiloProgram
    {
        static SiloConfig Config { get; set; } = new SiloConfig();

        public static int Main(string[] args)
        {
            ConfigSharp.Log.LogLevel = ConfigSharp.Log.Level.Info;
            ConfigSharp.Log.LogHandler = (lvl, ctx, msg) => { Console.WriteLine($"{lvl} {ctx} {msg}"); };
            Config.ParseCommandline(args);
            Config.Include(Config.ConfigFile);
            Console.WriteLine($"RunMode={Config.RunMode} ConfigSequence={Config.ConfigSequence}");

            return RunMainAsync().Result;
        }

        private static async Task<int> RunMainAsync()
        {
            try {
                var host = await StartSilo();
                Console.WriteLine("Press Enter to terminate...");
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
            var builder = new SiloHostBuilder();

            builder.Configure<ClusterOptions>(options => {
                options.ClusterId = Config.ClusterId;
                options.ServiceId = Cluster.ServiceId;
            });

            if (Config.LocalhostClustering) {
                builder.UseLocalhostClustering();
                builder.Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback);
            } else {
                builder.UseAzureStorageClustering(options => options.ConnectionString = Config.ClusteringAzureTableConnectionString);
                var name = Dns.GetHostName(); // get container id
                var ip = Dns.GetHostEntry(name).AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                //builder.Configure<EndpointOptions>(options => {
                //    options.GatewayListeningEndpoint = new IPEndPoint(IPAddress.Any, 30000);
                //    options.SiloListeningEndpoint = new IPEndPoint(IPAddress.Any, 11111);
                //});
                builder.ConfigureEndpoints(ip, 11111, 30000, true);
            }

            builder.ConfigureLogging(logging => {
                logging.AddConsole();

                if (Config.RunMode == SiloConfig.RunModes.Production) {
                    logging.SetMinimumLevel(LogLevel.Error);
                    //logging.SetMinimumLevel(LogLevel.Warning);
                }

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

            });

            builder.AddSimpleMessageStreamProvider(ItemService.StreamProvider, options => {
                options.FireAndForgetDelivery = true;
            });

            builder.AddMemoryGrainStorage(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME);

            builder.AddItemAzureTableStorage(
                name: ItemAzureTableStorage.StorageProviderName,
                configureOptions: options => {
                    options.TableName = "n3qItems";
                    options.ConnectionString = Config.ItemStateAzureTableConnectionString;
                });

            builder.AddReflectingAzureTableStorage(
                name: ReflectingAzureTableStorage.StorageProviderName,
                configureOptions: options => {
                    options.TableName = "n3qGrains";
                    options.ConnectionString = Config.GrainStateAzureTableConnectionString;
                });

            builder.AddAzureTableGrainStorage(
                name: Cluster.MemoryGrainStorageProviderName,
                configureOptions: options => {
                    options.UseJson = true;
                    options.ConnectionString = Config.PubsubStoreAzureTableConnectionString;
                });

            builder.UsePerfCounterEnvironmentStatistics();

            builder.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(TestStringGrain).Assembly).WithReferences());

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}
