using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SimpleB.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class CountryController : ControllerBase
    {
        private static readonly string[] Countries = new[]
        {
            "Australia", "India", "Chilly", "Italy", "Germany", "France", "Sri Lanka", "Argentina", "USA", "Brazil"
        };

        private readonly ILogger<CountryController> _logger;

        public CountryController(ILogger<CountryController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        //[Authorize(Roles = "Admin")]
        [Authorize(Policy = "DoSomeDBCheck")]
        public IEnumerable<Country> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new Country
            {
                Name = Countries[rng.Next(Countries.Length)]
            })
            .ToArray();
        }
    }
}
