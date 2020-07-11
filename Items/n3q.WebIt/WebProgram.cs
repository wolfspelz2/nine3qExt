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

namespace n3q.WebIt
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
                logging.AddConsole(c => {
                    c.TimestampFormat = "[yy:MM:dd-HH:mm:ss] ";
                });
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

            //var useIntegratedCluster = false;
            //if (useIntegratedCluster) {
            //    host.UseOrleans(builder => {

            //        if (config.LocalhostClustering) {
            //            builder.UseLocalhostClustering();
            //        } else {
            //            builder.UseAzureStorageClustering(options => options.ConnectionString = config.ClusteringAzureTableConnectionString);
            //        }

            //        builder.AddSimpleMessageStreamProvider(ItemService.StreamProvider, options => {
            //            options.FireAndForgetDelivery = true;
            //        });

            //        builder.AddMemoryGrainStorage(Cluster.MemoryGrainStorageProviderName);

            //        builder.AddMemoryGrainStorage(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME);

            //        builder.AddJsonFileStorage(
            //            name: JsonFileStorage.StorageProviderName,
            //            configureOptions: options => {
            //                options.RootDirectory = ItemService.JsonFileStorageRoot;
            //            });

            //        builder.UsePerfCounterEnvironmentStatistics();

            //        builder.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(TestStringGrain).Assembly).WithReferences());

            //    });
            //}

            return host;
        }
    }
}