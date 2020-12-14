using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Basket.API.ApiControllers
{
    [ApiController]
    [Route("[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly ILogger<SeedController> _logger;
        private readonly IDatabase _database;

        public SeedController(ILogger<SeedController> logger, IConnectionMultiplexer connectionMultiplexer)
        {
            _logger = logger;
            _database = connectionMultiplexer.GetDatabase();
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            await _database.StringIncrementAsync("test");

            var created = await _database.StringGetAsync("test");

            return Ok(created);
        }
    }
}
