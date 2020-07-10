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

namespace n3q.WebIt
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
            var config = new WebItConfigDefinition().Include(nameof(WebItConfig) + ".cs") as WebItConfigDefinition;
            config.Info((name, value) => ConfigSharp.Log.LogHandler(ConfigSharp.Log.Level.Info, "Config", $"{name}={value}"));
            services.AddSingleton<WebItConfigDefinition>(config);

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => {
            });

            services.AddAntiforgery(options => {
                options.SuppressXFrameOptionsHeader = true;
            });

            services.AddRazorPages();
            services.AddControllers();

            if (!config.UseIntegratedCluster) {
                services.AddSingleton<IClusterClient>((s) => {

                    var builder = new ClientBuilder();

                    if (config.LocalhostClustering) {
                        builder.UseLocalhostClustering();
                    } else {
                        builder.UseAzureStorageClustering(options => options.ConnectionString = config.ClusteringAzureTableConnectionString);
                    }

                    builder.Configure<ClusterOptions>(options => {
                        options.ClusterId = config.ClusterId;
                        options.ServiceId = Cluster.ServiceId;
                    });
                    builder.AddSimpleMessageStreamProvider(ItemService.StreamProvider);

                    var client = builder.Build();
                    client.Connect().Wait();
                    return client;
                });
            }

            services.AddTransient<ICommandline>(sp => { return new ItemCommandline(config); });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

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
