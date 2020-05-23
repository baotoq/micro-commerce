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
                    .RuleFor(_ => _.Name, _ =>
                    {
                        var c = _.PickRandom(categoriesName);
                        categoriesName.Remove(c);
                        return c;
                    });

                var productFaker = new Faker<Product>()
                    .RuleFor(_ => _.Name, _ => _.Commerce.Product())
                    .RuleFor(_ => _.Description, _ => _.Commerce.ProductAdjective())
                    .RuleFor(_ => _.ImageFileName, _ => _.Image.PicsumUrl())
                    .RuleFor(_ => _.Price, _ => decimal.Parse(_.Commerce.Price()));

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
