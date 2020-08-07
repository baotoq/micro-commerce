using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Bogus.DataSets;
using Bshop.Shared.V1;
using Catalog.API.Data;
using Catalog.API.Data.Models;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly PingService.PingServiceClient _client;

        public SeedController(ILogger<SeedController> logger, ApplicationDbContext context, PingService.PingServiceClient client)
        {
            _logger = logger;
            _context = context;
            _client = client;
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            await _client.PingAsync(new Empty());
            return Ok();
        }

        [HttpGet("migrate")]
        public async Task<IActionResult> Migrate()
        {
            await _context.Database.MigrateAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Seed()
        {
            _logger.LogInformation("Start seeding database");
            var categoriesName = new Commerce().Categories(20).Distinct().ToList();

            var categoryFaker = new Faker<Category>()
                .RuleFor(s => s.Name, s =>
                {
                    var c = s.PickRandom(categoriesName);
                    categoriesName.Remove(c);
                    return c;
                });

            var productFaker = new Faker<Product>()
                .RuleFor(s => s.Name, s => s.Commerce.Product())
                .RuleFor(s => s.Description, s => s.Lorem.Paragraph())
                .RuleFor(s => s.ImageUri, s => s.Image.PicsumUrl())
                .RuleFor(s => s.SellQuantity, s => 90)
                .RuleFor(s => s.StockQuantity, s => 100)
                .RuleFor(s => s.CartMaxQuantity, s => 10)
                .RuleFor(s => s.Price, s => decimal.Parse(s.Commerce.Price()));

            var categories = categoryFaker.Generate(5);

            foreach (var category in categories)
            {
                var products = productFaker.Generate(Randomizer.Seed.Next(50, 200));
                foreach (var product in products)
                {
                    category.Products.Add(new ProductCategory
                    {
                        Product = product
                    });
                }
            }

            await _context.Set<Category>().AddRangeAsync(categories);

            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeding database successful");

            return Ok();
        }
    }
}
