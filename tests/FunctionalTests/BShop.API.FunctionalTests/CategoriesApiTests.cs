using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BShop.API.Application.Categories.Commands.Create;
using BShop.API.Application.Categories.Models;
using BShop.API.FunctionalTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace BShop.API.FunctionalTests
{
    public class CategoriesApiTests : IClassFixture<TestWebApplicationFactory<Startup>>
    {
        private readonly TestWebApplicationFactory<Startup> _factory;

        public CategoriesApiTests(TestWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetAll_Success()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("api/categories");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<IEnumerable<CategoryDto>>();
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
            var result = await response.Content.ReadAsAsync<CategoryDto>();
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Create_NoAuthentication_Unauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            var category = new CreateCategoryCommand { Name = "Test no authentication" };

            // Act
            var response = await client.PostAsJsonAsync("api/categories", category);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Create_Authenticated_Success()
        {
            // Arrange
            var client = _factory.CreateAuthenticatedClient();
            var category = new CreateCategoryCommand { Name = "Test authenticated" };

            // Act
            var response = await client.PostAsJsonAsync("api/categories", category);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = await response.Content.ReadAsAsync<CategoryDto>();
            result.Should().BeEquivalentTo(new CategoryDto
            {
                Id = 2,
                Name = "Test authenticated"
            });
        }
    }
}
