using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MicroCommerce.Catalog.API.Application.Categories.Commands;
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
            var client = _factory.CreateUnauthenticatedClient(async context =>
            {
                await context.AddAsync(new Category());
                await context.AddAsync(new Category());
                await context.SaveChangesAsync();
            });

            // Act
            var response = await client.GetAsync(Uri);

            // Assert
            response.Should().Be200Ok().And
                .Satisfy<ICollection<CategoryDto>>(s => s.Should().NotBeNullOrEmpty());
        }

        [Fact]
        public async Task Create_Success()
        {
            // Arrange
            var client = _factory.CreateAuthenticatedClient(async context =>
            {
                await context.AddAsync(new Category());
                await context.SaveChangesAsync();
            });

            // Act
            var response = await client.PostAsJsonAsync(Uri, new CreateCategoryCommand
            { 
                Name = "IPhone"
            });

            // Assert
            response.Should().Be200Ok().And
                .Satisfy<CategoryDto>(s =>
                {
                    s.Id.Should().Be(2);
                    s.Name.Should().Be("IPhone");
                });
        }

        [Fact]
        public async Task Create_Unauthorized()
        {
            // Arrange
            var client = _factory.CreateUnauthenticatedClient();

            // Act
            var response = await client.PostAsJsonAsync(Uri, new CreateCategoryCommand
            {
                Name = "IPhone"
            });

            // Assert
            response.Should().Be401Unauthorized();
        }
    }
}
