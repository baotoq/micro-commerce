using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Mappers;
using MicroCommerce.Identity.Admin.EntityFramework.Shared.DbContexts;
using MicroCommerce.Identity.Web.Configuration.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace MicroCommerce.Identity.Web.Controllers
{
    [ApiController]
    public class SeedController : ControllerBase
    {
        private readonly IdentityServerConfigurationDbContext _configurationContext;
        private readonly IConfiguration _configuration;

        public SeedController(
            IdentityServerConfigurationDbContext configurationContext,
            IConfiguration configuration)
        {
            _configurationContext = configurationContext;
            _configuration = configuration;
        }

        [HttpGet("/seed")]
        public async Task<IActionResult> Seed()
        {
            if (!_configurationContext.Clients.Any())
            {
                await _configurationContext.Clients.AddRangeAsync(IdentityServerConfiguration.Clients(_configuration).Select(s => s.ToEntity()));
            }

            if (!_configurationContext.IdentityResources.Any())
            {
                await _configurationContext.IdentityResources.AddRangeAsync(IdentityServerConfiguration.IdentityResources.Select(s => s.ToEntity()));
            }

            if (!_configurationContext.ApiResources.Any())
            {
                await _configurationContext.ApiResources.AddRangeAsync(IdentityServerConfiguration.ApiResources.Select(s => s.ToEntity()));
            }

            if (!_configurationContext.ApiScopes.Any())
            {
                await _configurationContext.ApiScopes.AddRangeAsync(IdentityServerConfiguration.ApiScopes.Select(s => s.ToEntity()));
            }

            await _configurationContext.SaveChangesAsync();

            return Ok("Success");
        }
    }
}
