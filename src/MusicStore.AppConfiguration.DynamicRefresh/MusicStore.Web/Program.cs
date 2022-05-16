using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace MusicStore.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            // initialize an IHostBuilder class for a web application, including
            // root directory, host environment, and appsettings.
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                .ConfigureAppConfiguration((context, config) => {
                    var settings = config.Build();
                    var appConfigEndpoint = settings["AppSettings:AppConfiguration:Endpoint"];
                    var userAssignedIdentityClientId = settings["AppSettings:Identity:ClientId"];

                    if (!string.IsNullOrEmpty(appConfigEndpoint))
                    {
                        var endpoint = new Uri(appConfigEndpoint);

                        config.AddAzureAppConfiguration(options =>
                        {
                            options
                                .Connect(endpoint, new ManagedIdentityCredential(clientId: userAssignedIdentityClientId))
                                .ConfigureRefresh(refreshOpt =>
                                {
                                    // register a sentinel key and check *all* keys with a specific interval
                                    // will reload all configurations if registered key is modified.
                                    refreshOpt.Register(key: "AppSettings:Version", refreshAll: true, label: LabelFilter.Null)
                                    .SetCacheExpiration(TimeSpan.FromMinutes(10));
                                })
                                // enable feature flags
                                .UseFeatureFlags();
                            // override the default expiration policy
                            // .UseFeatureFlags(featureFlagOptions => featureFlagOptions.CacheExpirationTime = TimeSpan.FromMinutes(1));
                        });
                    }

                });
    }
}
