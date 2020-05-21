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
using nine3q.StorageProviders;
using nine3q.Grains;

namespace nine3q.Web
{
    public static class Config
    {
        public static bool UseIntegratedCluster = false;
    }

    public class Web
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
                });
            }

            return host;
        }
    }
}
