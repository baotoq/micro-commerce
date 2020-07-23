using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Catalog.API.Application.Categories.Commands;
using Catalog.API.Application.Categories.Commands.Create;
using Catalog.API.Application.Categories.Models;
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
    public class CategoriesApiTests : IClassFixture<TestWebApplicationFactory<Startup>>
    {
        private const string Uri = "api/categories";
        private readonly TestWebApplicationFactory<Startup> _factory;

        public CategoriesApiTests(TestWebApplicationFactory<Startup> factory)
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
            var result = await response.Content.ReadAsAsync<OffsetPaged<CategoryDto>>();
            result.Data.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task FindById_Success()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync($"{Uri}/1");

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
            var command = new CreateCategoryCommand { Name = "Test no authentication" };

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
            var command = new CreateCategoryCommand { Name = "Created" };

            // Act
            var response = await client.PostAsJsonAsync(Uri, command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = await response.Content.ReadAsAsync<CategoryDto>();
            result.Should().BeEquivalentTo(new CategoryDto
            {
                Name = "Created"
            }, s => s.Excluding(p => p.Id));
        }

        [Fact]
        public async Task Update_Authenticated_Success()
        {
            // Arrange
            var client = _factory.CreateAuthenticatedClient();
            var command = new UpdateCategoryCommand { Name = "Changed" };

            // Act
            var response = await client.PutAsJsonAsync($"{Uri}/1", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Delete_Authenticated_Success()
        {
            // Arrange
            var cat = new Category();

            var client = _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(async services =>
            {
                using var scope = services.BuildServiceProvider().CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.Categories.AddAsync(cat);
                await context.SaveChangesAsync();

            })).CreateAuthenticatedClient();

            // Act
            var response = await client.DeleteAsync($"{Uri}/{cat.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
