using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using n3q.Common;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace n3q.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var config = new WebConfig().Include("WebConfigRoot.cs") as WebConfig;
            services.AddSingleton<WebConfig>(config);

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => {
                });

            services.AddRazorPages();
            services.AddControllers();
            
            if (!config.UseIntegratedCluster) {
                services.AddSingleton<IClusterClient>((s) => {
                    var client = new ClientBuilder()
                        .UseLocalhostClustering()
                        .Configure<ClusterOptions>(options => {
                            options.ClusterId = Cluster.DevClusterId;
                            options.ServiceId = Cluster.ServiceId;
                        })
                        .AddSimpleMessageStreamProvider(ItemService.StreamProvider)
                        .Build();

                    client.Connect().Wait();
                    return client;
                });
            }

            services.AddTransient<ICommandline>(sp => { return new ItemCommandline(); });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles(options: new StaticFileOptions {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "../wwwroot")),
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}
