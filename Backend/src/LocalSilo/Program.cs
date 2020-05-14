﻿using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Statistics;
using Grains;

namespace LocalSilo
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return RunMainAsync().Result;
        }

        private static async Task<int> RunMainAsync()
        {
            try
            {
                var host = await StartSilo();
                Console.WriteLine("Press Enter to terminate...");
                Console.ReadLine();

                await host.StopAsync();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }
        }

        private static async Task<ISiloHost> StartSilo()
        {
            // define the cluster configuration
            var builder = new SiloHostBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "Sample";
                })
                .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Error);
                })
                .AddSimpleMessageStreamProvider("SMSProvider", options =>
                {
                    options.FireAndForgetDelivery = true;
                })
                .AddMemoryGrainStorage("PubSubStore")
                .UsePerfCounterEnvironmentStatistics()
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(TestString).Assembly).WithReferences())
                ;

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}
