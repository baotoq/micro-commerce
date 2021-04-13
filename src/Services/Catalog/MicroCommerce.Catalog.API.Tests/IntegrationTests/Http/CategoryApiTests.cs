using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MicroCommerce.Catalog.API.Application.Categories.Models;
using MicroCommerce.Catalog.API.Persistence.Entities;
using MicroCommerce.Catalog.API.Tests.IntegrationTests.Infrastructure;
using Xunit;

namespace MicroCommerce.Catalog.API.Tests.IntegrationTests.Http
{
    public class CategoryApiTests : IClassFixture<TestWebApplicationFactory<Startup>>
    {
        private const string Uri = "api/categories";
        private readonly TestWebApplicationFactory<Startup> _factory;
        private readonly Fixture _fixture;

        public CategoryApiTests(TestWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _fixture = new Fixture();
        }

        [Fact]
        public async Task Find_Success()
        {
            // Arrange
            var client = _factory.CreateInMemoryDbClient(async context =>
            {
                await context.AddAsync(new Category());
                await context.AddAsync(new Category());
                await context.SaveChangesAsync();
            });

            // Act
            var response = await client.GetAsync(Uri);

            // Assert
            response.Should().Be200Ok().And
                .Satisfy<ICollection<CategoryDto>>(s
                    => s.Should().NotBeNullOrEmpty());
        }
    }
}
