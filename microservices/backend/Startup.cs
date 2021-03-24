using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.FeatureManagement;
using backend.Services;

namespace backend
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
            services.AddControllers();
            services.AddSwaggerGen();
            services.AddStackExchangeRedisCache(o =>
            {
                var con = Configuration.GetConnectionString("redis");
                //default fallback
                if (con == null)
                    con = "localhost:6379";

                o.Configuration = con;
            });

            if (Configuration.UseFeatureManagement())
            {
                services.AddFeatureManagement();
                //services.AddAzureAppConfiguration();
            }

            services.AddHealthChecks(Configuration);
            services.AddOpenTelemetryTracing(Configuration);

            // External weather forecast API
            services.AddHttpClient<IAzureMapWeatherSvc, AzureMapWeatherSvc>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

            }

            // if (Configuration.UseFeatureManagement())
            // {
            //     app.UseAzureAppConfiguration();
            // }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "backend v1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
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
            var redisConnectionStr = configuration.GetConnectionString("redis");

            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy());
                //.AddRedis(redisConnectionStr, name: "redis-check", tags: new string[] { "redis" });

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
