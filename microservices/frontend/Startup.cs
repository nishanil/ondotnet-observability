using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using Microsoft.FeatureManagement;

namespace frontend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAppInsights(Configuration);
            services.AddRazorPages();
            services.AddHttpClient<WeatherClient>(client =>
            {
                var baseAddress = Configuration.GetServiceUri("backend");
                // if not running with tye, set to default
                if (baseAddress == null)
                    baseAddress = new Uri("http://localhost:9000");

                client.BaseAddress = baseAddress;
            });

            if (Configuration.UseFeatureManagement())
            {
                services.AddFeatureManagement();
               // services.AddAzureAppConfiguration();
            }

            services.AddHealthChecks(Configuration);
            services.AddOpenTelemetryTracing(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            var pathBase = Configuration["PATH_BASE"];
            if (!string.IsNullOrEmpty(pathBase))
            {
                app.UsePathBase(pathBase);
            }

            // if (Configuration.UseFeatureManagement())
            // {
            //     app.UseAzureAppConfiguration();
            // }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();

                endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
                {
                    Predicate = r => r.Name.Contains("self")
                });
            });
        }

        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions.HandleTransientHttpError()
                .WaitAndRetryAsync(5,
                    retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(1.5, retryAttempt) * 1000),
                    (_, waitingTime) =>
                    {
                        Console.WriteLine($"Retrying in {waitingTime.TotalSeconds}s");
                    });
        }

        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() =>
            HttpPolicyExtensions.HandleTransientHttpError()
                .CircuitBreakerAsync(15, TimeSpan.FromSeconds(15));
    }

    static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddOpenTelemetryTracing(this IServiceCollection services, IConfiguration configuration)
        {
            var zipkinEndpoint = configuration.GetServiceUri("zipkin");

            var exporter = configuration.GetValue<string>("UseExporter").ToLowerInvariant();
            var zipkinServiceName = configuration.GetValue<string>("Zipkin:ServiceName");

            if (!String.IsNullOrEmpty(exporter) && exporter == "zipkin")
            {
                services.AddOpenTelemetryTracing((builder) => builder
                        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(zipkinServiceName))
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddZipkinExporter(zipkinOptions =>
                        {
                            zipkinOptions.Endpoint = new Uri($"{zipkinEndpoint}api/v2/spans");
                        }));
            }
            else
            {
                services.AddOpenTelemetryTracing((builder) => builder
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddConsoleExporter());
            }


            return services;
        }

        public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            var backendBaseAddress = configuration.GetServiceUri("backend");

            if (backendBaseAddress == null)
                backendBaseAddress = new Uri("http://localhost:9000");

            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy())
                .AddUrlGroup(new Uri($"{backendBaseAddress}hc"), name: "backendapi-check", tags: new string[] { "backendapi" });

            return services;
        }

        public static IServiceCollection AddAppInsights(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplicationInsightsTelemetry(configuration);
            services.AddApplicationInsightsKubernetesEnricher();

            return services;
        }
    }

    static class IConfigurationExtensions
    {
        public static bool UseFeatureManagement(this IConfiguration configuration) =>
            configuration["UseFeatureManagement"] == bool.TrueString;
    }
}
