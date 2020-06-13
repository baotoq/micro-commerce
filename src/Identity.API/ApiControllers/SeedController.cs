using System.Linq;
using System.Threading.Tasks;
using Identity.API.Configurations;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Identity.API.ApiControllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly ILogger<SeedController> _logger;
        private readonly ConfigurationDbContext _configurationDbContext;
        private readonly IConfiguration _configuration;

        public SeedController(ILogger<SeedController> logger, ConfigurationDbContext configurationDbContext, IConfiguration configuration)
        {
            _logger = logger;
            _configurationDbContext = configurationDbContext;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Seed()
        {
            _logger.LogInformation("Start seeding database");

            if (_configurationDbContext.Clients.Any())
            {
                return Ok();
            }

            var clients = IdentityServerConfiguration.Clients(_configuration);
            var identityResource = IdentityServerConfiguration.IdentityResources;
            var apiResources = IdentityServerConfiguration.ApiResources;

            await _configurationDbContext.Clients.AddRangeAsync(clients.Select(s => s.ToEntity()));
            await _configurationDbContext.IdentityResources.AddRangeAsync(identityResource.Select(s => s.ToEntity()));
            await _configurationDbContext.ApiResources.AddRangeAsync(apiResources.Select(s => s.ToEntity()));

            await _configurationDbContext.SaveChangesAsync();
            _logger.LogInformation("Seeding database successful");

            return Ok();
        }
    }
}
