using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using MicroCommerce.Catalog.API.Application.Products.Models;
using MicroCommerce.Catalog.API.Infrastructure;
using MicroCommerce.Catalog.API.Persistence.Entities;
using MicroCommerce.Catalog.API.Tests.FunctionalTests.Infrastructure;
using Xunit;

namespace MicroCommerce.Catalog.API.Tests.FunctionalTests
{
    public class ProductApiTests : IClassFixture<TestWebApplicationFactory<Startup>>
    {
        private const string Uri = "api/products";
        private readonly TestWebApplicationFactory<Startup> _factory;

        public ProductApiTests(TestWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Find_Success()
        {
            // Arrange
            var client = _factory.CreateInMemoryDbClient(context =>
            {
                context.Add(new Product());
                context.SaveChanges();
            });

            // Act
            var response = await client.GetAsync(Uri);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<OffsetPaged<ProductDto>>();
            result.PaginationResult.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task FindById_Success()
        {
            // Arrange
            var client = _factory.CreateInMemoryDbClient(context =>
            {
                context.Add(new Product());
                context.SaveChanges();
            });

            // Act
            var response = await client.GetAsync(Uri + "/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<ProductDto>();
            result.Should().NotBeNull();
        }
    }
}
