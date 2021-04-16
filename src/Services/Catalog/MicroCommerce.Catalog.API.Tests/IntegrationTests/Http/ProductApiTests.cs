using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MicroCommerce.Catalog.API.Application.Products.Commands;
using MicroCommerce.Catalog.API.Application.Products.Models;
using MicroCommerce.Catalog.API.Infrastructure;
using MicroCommerce.Catalog.API.Infrastructure.Paged;
using MicroCommerce.Catalog.API.Persistence.Entities;
using MicroCommerce.Catalog.API.Tests.Helpers;
using MicroCommerce.Catalog.API.Tests.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace MicroCommerce.Catalog.API.Tests.IntegrationTests.Http
{
    public class ProductApiTests : IClassFixture<TestWebApplicationFactory<Startup>>
    {
        private const string BaseUrl = "api/products";
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
                await context.Products.AddAsync(new Product());
                await context.Products.AddAsync(new Product());
                await context.SaveChangesAsync();
            });

            // Act
            var response = await client.GetAsync(BaseUrl);

            // Assert
            response.Should().Be200Ok().And
                .Satisfy<OffsetPaged<ProductDto>>(s
                    => s.PaginationResult.Should().HaveCount(2));
        }

        [Fact]
        public async Task FindById_Success()
        {
            // Arrange
            var client = _factory.CreateInMemoryDbClient(async context =>
            {
                await context.Products.AddAsync(new Product());
                await context.SaveChangesAsync();
            });

            // Act
            var response = await client.GetAsync(UrlHelper.Combine(BaseUrl, "1"));

            // Assert
            response.Should().Be200Ok().And
                .Satisfy<ProductDto>(s => s.Should().NotBeNull());
        }

        [Fact]
        public async Task FindById_NotFound()
        {
            // Arrange
            var client = _factory.CreateInMemoryDbClient();

            // Act
            var response = await client.GetAsync(UrlHelper.Combine(BaseUrl, "1"));

            // Assert
            response.Should().Be404NotFound();
        }

        [Fact]
        public async Task Create_Success()
        {
            // Arrange
            var client = _factory.CreateAuthenticatedClient();

            var command = _fixture.Build<CreateProductCommand>()
                .Without(s => s.ImageFile)
                .Create();
            
            var multipart = new MultipartFormDataContent
            {
                {new StringContent(command.Name), nameof(command.Name)},
                {new StringContent(command.Price.ToString(CultureInfo.InvariantCulture)), nameof(command.Price)},
                {new StringContent(command.Description), nameof(command.Description)},
                {new StringContent(command.StockQuantity.ToString()), nameof(command.StockQuantity)},
                {new ByteArrayContent(Encoding.UTF8.GetBytes("This is a dummy file")), nameof(command.ImageFile), "image.jpg"},
            };

            // Act
            var response = await client.PostAsync(BaseUrl, multipart);

            // Assert
            response.Should().Be200Ok().And
                .Satisfy<ProductDto>(s => s.Should().NotBeNull());
        }

        [Fact]
        public async Task Create_Invalid()
        {
            // Arrange
            var client = _factory.CreateAuthenticatedClient();

            var multipart = new MultipartFormDataContent();

            // Act
            var response = await client.PostAsync(BaseUrl, multipart);

            // Assert
            response.Should().Be400BadRequest().And
                .Satisfy<ValidationProblemDetails>(s => s.Should().NotBeNull());
        }

        [Fact]
        public async Task Update_Success()
        {
            // Arrange
            var client = _factory.CreateAuthenticatedClient(async context =>
            {
                await context.Products.AddAsync(new Product());
                await context.SaveChangesAsync();
            });

            var command = _fixture.Build<UpdateProductCommand>()
                .With(s => s.Id, 1)
                .Without(s => s.ImageFile)
                .Create();
            
            var multipart = new MultipartFormDataContent
            {
                {new StringContent(command.Id.ToString()), nameof(command.Id)},
                {new StringContent(command.Name), nameof(command.Name)},
                {new StringContent(command.Price.ToString(CultureInfo.InvariantCulture)), nameof(command.Price)},
                {new StringContent(command.Description), nameof(command.Description)},
                {new StringContent(command.StockQuantity.ToString()), nameof(command.StockQuantity)},
                {new ByteArrayContent(Encoding.UTF8.GetBytes("This is a dummy file")), nameof(command.ImageFile), "image.jpg"},
            };

            // Act
            var response = await client.PutAsync(BaseUrl, multipart);

            // Assert
            response.Should().Be200Ok().And
                .Satisfy<ProductDto>(s => s.Should().NotBeNull());
        }

        [Fact]
        public async Task DeleteById_Success()
        {
            // Arrange
            var product = _fixture.Create<Product>();

            var client = _factory.CreateAuthenticatedClient(async context =>
            {
                await context.Products.AddAsync(product);
                await context.SaveChangesAsync();
            });

            // Act
            var response = await client.DeleteAsync(UrlHelper.Combine(BaseUrl, product.Id.ToString()));

            // Assert
            response.Should().Be200Ok();
        }

        [Fact]
        public async Task DeleteById_NotFound()
        {
            // Arrange
            var client = _factory.CreateAuthenticatedClient();

            // Act
            var response = await client.DeleteAsync(UrlHelper.Combine(BaseUrl, "1"));

            // Assert
            response.Should().Be404NotFound();
        }
    }
}
