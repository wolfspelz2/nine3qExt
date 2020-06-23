using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using ConfigSharp;

namespace n3q.Runtime
{
    public static class RuntimeProgram
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)

                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                })

                .ConfigureAppConfiguration((builderContext, config) => {
                    config.AddSharpConfiguration(options => {
                        options.ConfigFile = "ConfigRoot.cs";
                    });
                })
                
                ;
        }
    }
}
