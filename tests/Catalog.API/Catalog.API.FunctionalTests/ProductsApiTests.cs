using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Catalog.API.Application.Products.Commands;
using Catalog.API.Application.Products.Models;
using Catalog.API.Data;
using Catalog.API.Data.Models;
using Catalog.API.FunctionalTests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using UnitOfWork.Common;
using Xunit;

namespace Catalog.API.FunctionalTests
{
    public class ProductsApiTests : IClassFixture<TestWebApplicationFactory<Startup>>
    {
        private const string Uri = "api/products";
        private readonly TestWebApplicationFactory<Startup> _factory;

        public ProductsApiTests(TestWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Find_Success()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(Uri);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<OffsetPaged<ProductDto>>();
            result.Data.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task FindById_Success()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("api/categories/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<ProductDto>();
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Create_NoAuthentication_Unauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            var command = new CreateProductCommand { Name = "Test no authentication" };

            // Act
            var response = await client.PostAsJsonAsync(Uri, command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Create_Authenticated_Success()
        {
            // Arrange
            var client = _factory.CreateAuthenticatedClient();
            await using var file = new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file"));

            var command = new CreateProductCommand
            { 
                Name = "Created",
                Price = 100,
                Description = "Short description",
                CartMaxQuantity = 10,
                StockQuantity = 100,
                SellQuantity = 50
            };

            var content = new MultipartFormDataContent
            {
                { new ByteArrayContent(file.ToArray()), nameof(command.Image), "dummy.jpg" },
                { new StringContent(command.Name), nameof(command.Name) },
                { new StringContent(command.Price.ToString()), nameof(command.Price) },
                { new StringContent(command.Description), nameof(command.Description) },
                { new StringContent(command.CartMaxQuantity.ToString()), nameof(command.CartMaxQuantity) },
                { new StringContent(command.StockQuantity.ToString()), nameof(command.StockQuantity) },
                { new StringContent(command.SellQuantity.ToString()), nameof(command.SellQuantity) },
            };

            // Act
            var response = await client.PostAsync(Uri, content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = await response.Content.ReadAsAsync<ProductDto>();
            result.Should().BeEquivalentTo(new ProductDto
            {
                Name = "Created"
            }, s => s.Including(p => p.Name));
        }

        [Fact]
        public async Task Update_Authenticated_Success()
        {
            // Arrange
            var client = _factory.CreateAuthenticatedClient();
            var command = new UpdateProductCommand
            {
                Name = "Changed",
                Price = 100,
                StockQuantity = 100,
            };

            var content = new MultipartFormDataContent
            {
                { new StringContent(command.Name), nameof(command.Name) },
                { new StringContent(command.Price.ToString()), nameof(command.Price) },
                { new StringContent(command.StockQuantity.ToString()), nameof(command.StockQuantity) },
            };

            // Act
            var response = await client.PutAsync($"{Uri}/1", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Delete_Authenticated_Success()
        {
            // Arrange
            var product = new Product
            {
                ImageUri = "test.jpg"
            };

            var client = _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(async services =>
            {
                using var scope = services.BuildServiceProvider().CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.Products.AddAsync(product);
                await context.SaveChangesAsync();

            })).CreateAuthenticatedClient();

            // Act
            var response = await client.DeleteAsync($"{Uri}/{product.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
