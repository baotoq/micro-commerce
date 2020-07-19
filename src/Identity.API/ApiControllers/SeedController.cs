using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Identity.API.Configurations;
using Identity.API.Data.Models;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private const string Password = "1qazZAQ!";

        public SeedController(ILogger<SeedController> logger, ConfigurationDbContext configurationDbContext, IConfiguration configuration, UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            _logger = logger;
            _configurationDbContext = configurationDbContext;
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet("identity-server-config")]
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

        [HttpGet("roles")]
        public async Task<IActionResult> SeedRole()
        {
            _logger.LogInformation("Start seeding database");

            await _roleManager.CreateAsync(new Role { Name = "Admin" });
            await _roleManager.CreateAsync(new Role { Name = "Customer" });

            _logger.LogInformation("Seeding database successful");

            return Ok();
        }

        [HttpGet("users-customer")]
        public async Task<IActionResult> SeedUserCustomer(int count = 20)
        {
            _logger.LogInformation("Start seeding database");

            var customer = await _roleManager.FindByNameAsync("Customer");

            var userFaker = new Faker<User>()
                .RuleFor(s => s.UserName, s => s.Person.Email)
                .RuleFor(s => s.Email, s => s.Person.Email)
                .RuleFor(s => s.Roles, s => new List<UserRole>() { new UserRole { RoleId = customer.Id } });

            var users = userFaker.Generate(count).ToList();

            foreach (var item in users)
            {
                await _userManager.CreateAsync(item, Password);
            }

            _logger.LogInformation("Seeding database successful");

            return Ok();
        }

        [HttpGet("users-admin")]
        public async Task<IActionResult> SeedUserAmin(int count = 20)
        {
            _logger.LogInformation("Start seeding database");

            var admin = await _roleManager.FindByNameAsync("Admin");

            await _userManager.CreateAsync(new User
            {
                UserName = $"admin@gmail.com",
                Email = $"admin@gmail.com",
                Roles = new List<UserRole>() { new UserRole { RoleId = admin.Id } }
            }, Password);

            for (int i = 0; i < count; i++)
            {
                await _userManager.CreateAsync(new User
                {
                    UserName = $"admin{i}@gmail.com",
                    Email = $"admin{i}@gmail.com",
                    Roles = new List<UserRole>() { new UserRole { RoleId = admin.Id } }
                }, Password);
            }

            _logger.LogInformation("Seeding database successful");

            return Ok();
        }
    }
}
