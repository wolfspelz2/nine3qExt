using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Statistics;
using Orleans;
using n3q.Common;
using n3q.StorageProviders;
using n3q.Grains;
using ConfigSharp;

namespace n3q.Web
{
    public static class WebProgram
    {
        public static void Main(string[] args)
        {
            ConfigSharp.Log.LogLevel = ConfigSharp.Log.Level.Info;
            ConfigSharp.Log.LogHandler = (lvl, ctx, msg) => { Console.WriteLine($"{lvl} {ctx} {msg}"); };

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
                webBuilder
                    .UseUrls("http://*:25343")
                    .UseStartup<Startup>();
            });

            var useIntegratedCluster = false;
            if (useIntegratedCluster) {
                host.UseOrleans(builder => {
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
