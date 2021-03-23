using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Diagnostics;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry;
using Microsoft.FeatureManagement;
using backend.Services;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly IFeatureManager _featureManager;
        private readonly ILogger<WeatherForecastController> _logger;
        private ActivitySource _activitySource = new ActivitySource(nameof(WeatherForecastController));
        private IAzureMapWeatherSvc _azureMapWeatherSvc;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IFeatureManager featureManager, IAzureMapWeatherSvc azureMapWeatherSvc)
        {
            _logger = logger;
            _featureManager = featureManager;
            _azureMapWeatherSvc = azureMapWeatherSvc;
        }

        // Implementation without cache

        //[HttpGet]
        //public async Task<string> Get([FromServices] IDistributedCache cache, string query = "12.9716,77.5946")
        //{
        //    try
        //    {
        //        using (var activity = _activitySource.StartActivity("GET:Recevied", ActivityKind.Server))
        //        {

        //            _logger.LogInformation("{Method} - was called ", "backend.Controllers.WeatherForecastController.Get");

        //            // Fetching data from cache
        //            var weather = string.Empty;

        //            if (await _featureManager.IsEnabledAsync("ExternalWeatherAPI"))
        //            {
        //                _logger.LogInformation("Calling external api");
        //                weather = await GetWeatherExternalData(query);
        //            }
        //            else
        //            {
        //                _logger.LogInformation("Fetching static data");
        //                weather = await GetWeatherStaticData();
        //            }

        //            _logger.LogInformation("Weather data - {data}", weather);

        //            _logger.LogInformation("Weather data fetched !");

        //            return weather;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Unable to fetch weather data !", ex);
        //        throw;
        //    }
        //}

        [HttpGet]
        public async Task<string> Get([FromServices] IDistributedCache cache, string query = "12.9716,77.5946")
        {
            try
            {
                using (var activity = _activitySource.StartActivity("GET:Recevied", ActivityKind.Server))
                {                    
                    _logger.LogInformation("{Method} - was called ", "backend.Controllers.WeatherForecastController.Get");
                    
                    // Fetching data from cache
                    var weather = await cache.GetStringAsync("weather");
                    
                    if (weather == null)
                    {
                        _logger.LogInformation("Cache Empty !");

                        if (await _featureManager.IsEnabledAsync("ExternalWeatherAPI"))
                        {
                            _logger.LogInformation("Calling external api");
                            weather = await GetWeatherExternalData(query);
                        }
                        else
                        {
                            _logger.LogInformation("Fetching static data");
                            weather = await GetWeatherStaticData();
                        }

                        _logger.LogInformation("Updating cache value");

                        await cache.SetStringAsync("weather", weather, new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1)
                        });
                    }
                    else
                    {
                        _logger.LogInformation("Weather data found in cache !");
                    }

                    _logger.LogInformation("Weather data - {data}", weather);

                    _logger.LogInformation("Weather data fetched !");

                    return weather;

                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Unable to fetch weather data !", ex);
                throw;
            }
        }

        private async Task<string> GetWeatherStaticData()
        {
            var rng = new Random();
            var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();

            return JsonSerializer.Serialize(forecasts);
        }

        private async Task<string> GetWeatherExternalData(string query)
        {
            var modifiedWeatherData = await _azureMapWeatherSvc.GetAzureMapWeatherForcastDate(query);
            return JsonSerializer.Serialize(modifiedWeatherData);
        }
    }
}
