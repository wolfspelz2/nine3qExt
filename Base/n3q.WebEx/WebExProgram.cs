using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace n3q.WebEx
{
    public static class WebExProgram
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
            });

            host.ConfigureWebHostDefaults(webBuilder => {
                webBuilder.UseStartup<Startup>();
            });

            return host;
        }
    }
}
