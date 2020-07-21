using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Catalog.API.Application.Categories.Commands.Create;
using Catalog.API.Application.Categories.Models;
using Catalog.API.Application.Reviews.Commands;
using Catalog.API.Application.Reviews.Models;
using Catalog.API.FunctionalTests.Infrastructure;
using Catalog.API.Data.Models.Enums;
using FluentAssertions;
using UnitOfWork.Common;
using Xunit;

namespace Catalog.API.FunctionalTests
{
    public class ReviewsApiTests : IClassFixture<TestWebApplicationFactory<Startup>>
    {
        private const string Uri = "api/reviews";
        private readonly TestWebApplicationFactory<Startup> _factory;

        public ReviewsApiTests(TestWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Create_NoAuthentication_Unauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            var command = new CreateReviewCommand { Title = "Test no authentication" };

            // Act
            var response = await client.PostAsJsonAsync(Uri, command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Update_Authenticated_Success()
        {
            // Arrange
            var client = _factory.CreateAuthenticatedClient();
            var command = new ChangeReviewStatusCommand { ReviewStatus = ReviewStatus.Approved };

            // Act
            var response = await client.PostAsJsonAsync($"{Uri}/1/change-review-status", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Delete_Authenticated_Success()
        {
            // Arrange
            var client = _factory.CreateAuthenticatedClient();

            // Act
            var response = await client.DeleteAsync($"{Uri}/2");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
