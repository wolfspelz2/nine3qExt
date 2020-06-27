using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

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
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}
