using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Catalog.API.Application.Products.Commands.Create;
using Catalog.API.Application.Products.Commands.Put;
using Catalog.API.Application.Products.Models;
using Catalog.API.FunctionalTests.Infrastructure;
using FluentAssertions;
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
            var result = await response.Content.ReadAsAsync<IEnumerable<ProductDto>>();
            result.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Get_Success()
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
            var command = new CreateProductCommand { Name = "Created" };

            // Act
            var response = await client.PostAsJsonAsync(Uri, command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = await response.Content.ReadAsAsync<ProductDto>();
            result.Should().BeEquivalentTo(new ProductDto
            {
                Name = "Created"
            }, s => s.Excluding(p => p.Id));
        }

        [Fact]
        public async Task Put_Authenticated_Success()
        {
            // Arrange
            var client = _factory.CreateAuthenticatedClient();
            var command = new PutProductCommand { Name = "Changed" };

            // Act
            var response = await client.PutAsJsonAsync($"{Uri}/1", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Delete_Authenticated_Success()
        {
            // Arrange
            var client = _factory.CreateAuthenticatedClient();

            // Act
            var response = await client.DeleteAsync($"{Uri}/2");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
