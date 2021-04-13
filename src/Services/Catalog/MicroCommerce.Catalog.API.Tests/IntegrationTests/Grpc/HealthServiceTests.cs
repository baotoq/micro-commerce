using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Grpc.Health.V1;
using MicroCommerce.Catalog.API.Tests.IntegrationTests.Infrastructure;
using Xunit;

namespace MicroCommerce.Catalog.API.Tests.IntegrationTests.Grpc
{
    public class HealthServiceTests : IClassFixture<TestWebApplicationFactory<Startup>>
    {
        private readonly TestWebApplicationFactory<Startup> _factory;
        private readonly Fixture _fixture;

        public HealthServiceTests(TestWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CheckTest()
        {
            var client = _factory.CreateGrpcClient<Health.HealthClient>();

            var act = await client.CheckAsync(new HealthCheckRequest());

            act.Status.Should().Be(HealthCheckResponse.Types.ServingStatus.Serving);
        }
    }
}
