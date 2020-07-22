using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bshop.V1.Identity;
using Catalog.API.Application.Reviews.Commands;
using Catalog.API.Application.Reviews.Models;
using Catalog.API.Data;
using Catalog.API.Data.Models;
using Catalog.API.FunctionalTests.Infrastructure;
using Catalog.API.Data.Models.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shared.Testings;
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
        public async Task Find_Success()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            {
                var identityClientMock = new Mock<IdentityService.IdentityServiceClient>();
                identityClientMock.Setup(s => s.GetUsersByIdsAsync(It.IsAny<GetUsersByIdsRequest>(),
                        null, null, It.IsAny<CancellationToken>()))
                    .Returns(GrpcTestCalls.AsyncUnaryCall(new GetUsersByIdsResponse
                    {
                        Users = { new GetUsersByIdsResponse.Types.User { Id = MasterData.CurrentUserId, Email = "test@gmail.com", UserName = "test@gmail.com" } }
                    }));

                services.AddSingleton(identityClientMock.Object);
            })).CreateClient();

            // Act
            var response = await client.GetAsync(Uri + "/offset");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<OffsetPaged<ReviewDto>>();
            result.Data.Should().NotBeNullOrEmpty();
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
            var review = new Review();

            var client = _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(async services =>
            {
                using var scope = services.BuildServiceProvider().CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.Reviews.AddAsync(review);
                await context.SaveChangesAsync();

            })).CreateAuthenticatedClient();

            // Act
            var response = await client.DeleteAsync($"{Uri}/{review.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
