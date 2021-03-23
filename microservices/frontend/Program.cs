using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace frontend
{
    public class Program
    {
        const string APP_NAME = "frontend";

        public static void Main(string[] args)
        {
            var configuration = GetConfiguration();
            Log.Logger = CreateSerilogLogger(configuration);

            try
            {
                Log.Information("Configuring web host ({ApplicationContext})...", APP_NAME);

                Activity.DefaultIdFormat = ActivityIdFormat.W3C;
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", APP_NAME);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureAppConfiguration((builderContext, configBuilder) =>
                {
                    configBuilder.AddEnvironmentVariables();
                    var settings = configBuilder.Build();
                    if (settings.GetValue<bool>("UseFeatureManagement") && !string.IsNullOrEmpty(settings["AppConfig:Endpoint"]))
                    {
                        configBuilder.AddAzureAppConfiguration(options =>
                        {
                            var cacheTime = TimeSpan.FromSeconds(5);

                            options.Connect(settings["AppConfig:Endpoint"])
                                .UseFeatureFlags(flagOptions =>
                                {
                                    flagOptions.CacheExpirationInterval = cacheTime;
                                })
                                .ConfigureRefresh(refreshOptions =>
                                {
                                    refreshOptions.Register("FeatureManagement:ExternalWeatherAPI", refreshAll: true)
                                                  .SetCacheExpiration(cacheTime);
                                });
                        });
                    }
                });

        private static IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            return builder.Build();
        }

        private static Serilog.ILogger CreateSerilogLogger(IConfiguration configuration)
        {
            var seqServerUrl = configuration["Serilog:SeqServerUrl"];
            var instrumentationKey = configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];

            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithProperty("ApplicationContext", APP_NAME)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.ApplicationInsights(instrumentationKey, TelemetryConverter.Traces)
                .WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl)                
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }
    }
}
