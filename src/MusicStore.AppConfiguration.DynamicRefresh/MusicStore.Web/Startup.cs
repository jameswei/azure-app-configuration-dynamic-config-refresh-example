using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MusicStore.Data;
using MusicStore.Services;
using Microsoft.FeatureManagement;
using MusicStore.Web.FeatureManagement.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MusicStore.Web.FeatureManagement;
using MusicStore.Shared;
using Microsoft.Extensions.Hosting;

namespace MusicStore.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to register services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Bind configuration data to POCO class
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            // Configure custom injections
            services.AddScoped<IAlbumService, AlbumService>();
            services.AddScoped<IAlbumRepository, AlbumRepository>();

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Configure Feature Management
            services
                .AddFeatureManagement()
                .AddFeatureFilter<BrowserFilter>()
                .UseDisabledFeaturesHandler(new MusicStoreDisabledFeaturesHandler());

            services.AddControllersWithViews();
            // add App Configuration components to the service collection.
            services.AddAzureAppConfiguration();
            // register controllers for rest api.
            // services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAzureAppConfiguration();

            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            // HSTS Http Restrict Transportation Secure Protocol, which is a response header to prevent browser sending any request over HTTP.
            app.UseHsts();

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
