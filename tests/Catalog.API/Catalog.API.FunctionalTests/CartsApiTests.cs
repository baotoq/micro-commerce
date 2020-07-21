using Catalog.API.FunctionalTests.Infrastructure;
using Xunit;

namespace Catalog.API.FunctionalTests
{
    public class CartsApiTests : IClassFixture<TestWebApplicationFactory<Startup>>
    {
        private const string Uri = "api/carts";
        private readonly TestWebApplicationFactory<Startup> _factory;

        public CartsApiTests(TestWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }
    }
}
