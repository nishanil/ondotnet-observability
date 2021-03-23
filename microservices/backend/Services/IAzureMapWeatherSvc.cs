using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services
{
    public interface IAzureMapWeatherSvc
    {
        public Task<WeatherForecast[]> GetAzureMapWeatherForcastDate(string query);
    }
}
