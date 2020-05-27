using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Bogus.DataSets;
using BShop.API.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BShop.API.Data
{
    public static class ApplicationDbContextSeeder
    {
        public static async Task InitializeDataAsync(this DbContext context)
        {
            if (!context.Set<Category>().Any())
            {
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
                    .RuleFor(s => s.Description, s => s.Commerce.ProductAdjective())
                    .RuleFor(s => s.ImageFileName, s => s.Image.PicsumUrl())
                    .RuleFor(s => s.Price, s => decimal.Parse(s.Commerce.Price()));

                var categories = categoryFaker.Generate(10);

                foreach (var category in categories)
                {
                    var products = productFaker.Generate(Randomizer.Seed.Next(10, 30));
                    foreach (var product in products)
                    {
                        category.ProductCategories.Add(new ProductCategory
                        {
                            Product = product
                        });
                    }
                }

                await context.Set<Category>().AddRangeAsync(categories);

                await context.SaveChangesAsync();
            }
        }
    }
}
