using System.Collections.Generic;
using Catalog.API.Data;
using Catalog.API.Data.Models;

namespace Catalog.API.FunctionalTests.Infrastructure
{
    public static class DbContextUtilities
    {
        public static void InitializeDbForTests(this ApplicationDbContext context)
        {
            var categories = new List<Category>
            {
                new Category
                {
                    Name = "Test category 1"
                },
                new Category
                {
                    Name = "Test category 2"
                }
            };

            context.Categories.AddRange(categories);

            context.Products.Add(new Product
            {
                Name = "Test product 1",
                ImageUri = "image.jpg"
            });

            context.Products.Add(new Product
            {
                Name = "Test product 2",
                ImageUri = "image2.jpg"
            });

            context.Reviews.Add(new Review
            {
                Title = "Test review 1",
                Rating = 2,
                ProductId = 1,
                CreatedById = MasterData.CurrentUserId
            });

            context.Reviews.Add(new Review
            {
                Title = "Test review 2",
                Rating = 5,
                ProductId = 1,
                CreatedById = MasterData.CurrentUserId
            });

            context.SaveChanges();
        }
    }
}
