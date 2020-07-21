using Catalog.API.FunctionalTests.Infrastructure;
using Xunit;

namespace Catalog.API.FunctionalTests
{
    public class OrdersApiTests : IClassFixture<TestWebApplicationFactory<Startup>>
    {
        private const string Uri = "api/orders";
        private readonly TestWebApplicationFactory<Startup> _factory;

        public OrdersApiTests(TestWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }
    }
}
