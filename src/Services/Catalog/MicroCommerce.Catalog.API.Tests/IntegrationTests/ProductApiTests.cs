using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MicroCommerce.Catalog.API.Application.Products.Commands;
using MicroCommerce.Catalog.API.Application.Products.Models;
using MicroCommerce.Catalog.API.Infrastructure;
using MicroCommerce.Catalog.API.Persistence.Entities;
using MicroCommerce.Catalog.API.Tests.IntegrationTests.Infrastructure;
using Xunit;

namespace MicroCommerce.Catalog.API.Tests.IntegrationTests
{
    public class ProductApiTests : IClassFixture<TestWebApplicationFactory<Startup>>
    {
        private const string Uri = "api/products";
        private readonly TestWebApplicationFactory<Startup> _factory;
        private readonly Fixture _fixture;

        public ProductApiTests(TestWebApplicationFactory<Startup> factory)
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
                await context.AddAsync(new Product());
                await context.SaveChangesAsync();
            });

            // Act
            var response = await client.GetAsync(Uri);

            // Assert
            response.Should().Be200Ok().And
                .Satisfy<OffsetPaged<ProductDto>>(s
                    => s.PaginationResult.Should().NotBeNullOrEmpty());
        }

        [Fact]
        public async Task FindById_Success()
        {
            // Arrange
            var client = _factory.CreateInMemoryDbClient(async context =>
            {
                await context.AddAsync(new Product());
                await context.SaveChangesAsync();
            });

            // Act
            var response = await client.GetAsync(Uri + "/1");

            // Assert
            response.Should().Be200Ok().And
                .Satisfy<ProductDto>(s => s.Should().NotBeNull());
        }

        [Fact]
        public async Task Create_Success()
        {
            // Arrange
            var client = _factory.CreateInMemoryDbClient();

            // Act
            var response = await client.PostAsJsonAsync(Uri, _fixture.Create<CreateProductCommand>());

            // Assert
            response.Should().Be200Ok().And
                .Satisfy<ProductDto>(s => s.Should().NotBeNull());
        }

        [Fact]
        public async Task Update_Success()
        {
            // Arrange
            var client = _factory.CreateInMemoryDbClient(async context =>
            {
                await context.AddAsync(new Product());
                await context.SaveChangesAsync();
            });

            // Act
            var response = await client.PutAsJsonAsync(Uri, 
                _fixture.Build<UpdateProductCommand>()
                .With(s => s.Id, 1)
                .Create()
            );

            // Assert
            response.Should().Be200Ok().And
                .Satisfy<ProductDto>(s => s.Should().NotBeNull());
        }

        [Fact]
        public async Task DeleteById_Success()
        {
            // Arrange
            var client = _factory.CreateInMemoryDbClient(async context =>
            {
                await context.AddAsync(new Product());
                await context.SaveChangesAsync();
            });

            // Act
            var response = await client.DeleteAsync(Uri + "/1");

            // Assert
            response.Should().Be200Ok();
        }
    }
}
