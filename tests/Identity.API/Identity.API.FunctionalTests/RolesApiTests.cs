using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Identity.API.Application.Roles;
using Identity.API.FunctionalTests.Infrastructure;
using Xunit;

namespace Identity.API.FunctionalTests
{
    public class RolesApiTests : IClassFixture<TestWebApplicationFactory<Startup>>
    {
        private const string Uri = "api/roles";
        private readonly TestWebApplicationFactory<Startup> _factory;

        public RolesApiTests(TestWebApplicationFactory<Startup> factory)
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
            var result = await response.Content.ReadAsAsync<List<RoleDto>>();
            result.Should().NotBeNullOrEmpty();
        }
    }
}
