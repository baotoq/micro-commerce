using System.Threading.Tasks;
using Catalog.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Catalog.API.ApiControllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly ILogger<SeedController> _logger;
        private readonly ApplicationDbContext _context;

        public SeedController(ILogger<SeedController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Seed()
        {
            _logger.LogInformation("Start seeding database");
            await _context.InitializeDataAsync();
            _logger.LogInformation("Seeding database was successful");

            return Ok();
        }
    }
}
