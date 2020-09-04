using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Data.Entities.Common;
using FluentAssertions;
using Identity.API.Application.Users.Models;
using Identity.API.FunctionalTests.Infrastructure;
using Xunit;

namespace Identity.API.FunctionalTests
{
    public class UsersApiTests : IClassFixture<TestWebApplicationFactory<Startup>>
    {
        private const string Uri = "api/users";
        private readonly TestWebApplicationFactory<Startup> _factory;

        public UsersApiTests(TestWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Find_Success()
        {
            // Arrange
            var client = _factory.CreateAuthenticatedClient();

            // Act
            var response = await client.GetAsync(Uri);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<OffsetPaged<UserDto>>();
            result.Data.Should().NotBeNullOrEmpty();
        }
    }
}
