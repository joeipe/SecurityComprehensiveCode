using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SimpleSpa.NgClient.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public WeatherForecastController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var sampleBClient = _httpClientFactory.CreateClient("SampleA_APIClient");
            var response = await sampleBClient.GetAsync("WeatherForecast/").ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                string jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<List<WeatherForecast>>(jsonString, options);
                return Ok(result);
            }
            else
            {
                return StatusCode((int)response.StatusCode);
            }
        }
    }
}
