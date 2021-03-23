using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Context.Propagation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace backend.Services
{
    public class AzureMapWeatherSvc : IAzureMapWeatherSvc
    {        
        private const string AZURE_MAP_WEATHER_API_BASE_URL = "https://atlas.microsoft.com/weather/currentConditions/json?";
        private ActivitySource _activitySource = new ActivitySource(nameof(AzureMapWeatherSvc));
        private readonly ILogger<AzureMapWeatherSvc> _logger;

        private readonly HttpClient _client;
        private IConfiguration _configuration;

        public AzureMapWeatherSvc(ILogger<AzureMapWeatherSvc> logger, HttpClient client , IConfiguration configuration)
        {
            _logger = logger;
            _client = client;
            _configuration = configuration;
        }

        private readonly JsonSerializerOptions options = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public async Task<WeatherForecast[]> GetAzureMapWeatherForcastDate(string query)
        {
            try
            {
                using (var activity = _activitySource.StartActivity("GET:Recevied", ActivityKind.Consumer))
                {
                    var apiVersion = "1.0";
                    //var query = "12.9716,77.5946";
                    var duration = "6"; // 6 in days                    
                    var subscriptionKey = _configuration.GetValue<string>("AzureMapSubscriptionKey");
                    var weatherforecastUrl = $"{AZURE_MAP_WEATHER_API_BASE_URL}api-version={apiVersion}&query={query}&duration={duration}&subscription-key={subscriptionKey}";

                    activity?.AddEvent(new ActivityEvent("GetAzureMapWeatherDataAsync:Started"));

                    var responseMessage = await _client.GetAsync(weatherforecastUrl);

                    _logger.LogInformation("Was able to fetch data from Azure Weather API !");

                    activity?.AddEvent(new ActivityEvent("GetAzureMapWeatherDataAsync:Ended"));

                    if (responseMessage != null)
                    {
                        var jsonString = await responseMessage.Content.ReadAsStringAsync();
                        var azureWeatherForecastMapData = JsonSerializer.Deserialize<AzureWeatherForecast.Rootobject>(jsonString, options);
                        var topExternalWeatherResults = GetMappedWeatherForecastData(azureWeatherForecastMapData);

                        return topExternalWeatherResults.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError("Unable to fetch the external weather data !");

                throw;
            }
            return new WeatherForecast[] { };
        }

        private List<WeatherForecast> GetMappedWeatherForecastData(AzureWeatherForecast.Rootobject azureWeatherForecastMapData)
        {
            int count = 1;

            List<WeatherForecast> topExternalWeatherResults = new List<WeatherForecast>();

            foreach (var item in azureWeatherForecastMapData.results)
            {
                if (count > 3) break;

                topExternalWeatherResults.Add(new WeatherForecast() { Date = item.dateTime, TemperatureC = Convert.ToInt32(item.realFeelTemperature.value), Summary = item.phrase });

                count += 1;
            }

            return topExternalWeatherResults;
        }
    }
}
