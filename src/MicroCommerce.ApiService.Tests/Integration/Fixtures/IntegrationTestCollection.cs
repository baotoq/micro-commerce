namespace MicroCommerce.ApiService.Tests.Integration.Fixtures;

/// <summary>
/// xUnit collection fixture to share ApiWebApplicationFactory across all integration test classes.
/// This ensures one PostgreSQL container is created for all tests, improving performance.
/// </summary>
[CollectionDefinition("Integration Tests")]
public class IntegrationTestCollection : ICollectionFixture<ApiWebApplicationFactory>
{
    // This class is never instantiated - it's just a marker for xUnit collection fixture
}
