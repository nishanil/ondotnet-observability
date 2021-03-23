using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace frontend.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public WeatherForecast[] Forecasts { get; set; }

        public string ErrorMessage {get;set;}
        
        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGet([FromServices]WeatherClient client)
        {
            string query = "12.9716,77.5946";

            if (!String.IsNullOrEmpty(Request.Query["query"]))
                query = Request.Query["query"];

            Console.WriteLine($"query -- {query}");

            Forecasts = await client.GetWeatherAsync(query);
            
            if(Forecasts.Count()==0)
                ErrorMessage="We are unable to fetch weather info right now. Please try again after some time.";
            else
                ErrorMessage = string.Empty;
        }
    }
}
