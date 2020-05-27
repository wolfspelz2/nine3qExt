using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Statistics;
using Orleans;
using n3q.Common;
using n3q.StorageProviders;
using n3q.Grains;

namespace n3q.Web
{
    public static class Config
    {
        public static bool UseIntegratedCluster = false;
    }

    public static class Web
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args);

            host.ConfigureLogging(logging => {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Warning);
                logging.AddFilter((provider, category, logLevel) => {
                    if (category.Contains("Orleans")) {
                        if (category.Contains(" Orleans.Hosting.SiloHostedService")) {
                            if (logLevel >= LogLevel.Information) {
                                return true;
                            }
                        } else {
                            if (logLevel >= LogLevel.Warning) {
                                return true;
                            }
                        }
                    } else if (logLevel >= LogLevel.Information) {
                        return true;
                    }
                    return false;
                });
            });

            host.ConfigureWebHostDefaults(webBuilder => {
                webBuilder.UseStartup<Startup>();
            });

            if (Config.UseIntegratedCluster) {
                host.UseOrleans(builder => {
                    // EnableDirectClient is no longer needed as it is enabled by default
                    builder.UseLocalhostClustering()

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
                });
            }

            return host;
        }
    }
}
