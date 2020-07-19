using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Bogus.DataSets;
using Catalog.API.Data;
using Catalog.API.Data.Models;
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
