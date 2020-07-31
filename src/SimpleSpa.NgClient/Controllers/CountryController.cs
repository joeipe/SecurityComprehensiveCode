using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SimpleSpa.NgClient.ViewModels;

namespace SimpleSpa.NgClient.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CountryController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "sub").Value;

            var sampleBClient = _httpClientFactory.CreateClient("SampleB_APIClient");
            var response = await sampleBClient.GetAsync("Country/").ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                string jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<List<Country>>(jsonString, options);
                return Ok(result);
            }
            else
            {
                return StatusCode((int)response.StatusCode);
            }
        }
    }
}
