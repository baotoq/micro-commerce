using System.Net;
using System.Net.Http.Json;

namespace MicroCommerce.IntegrationTests;

public class ProductsApiTests(AppHostFixture fixture)
{
    private sealed record Product(Guid Id, string Name, decimal Price, DateTimeOffset CreatedAt);

    private readonly HttpClient _client = fixture.ApiClient;

    [Fact]
    public async Task Health_endpoint_is_healthy()
    {
        var response = await _client.GetAsync("/health", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Product_round_trips_through_postgres_and_redis()
    {
        var ct = TestContext.Current.CancellationToken;

        var createResponse = await _client.PostAsJsonAsync("/products", new { Name = "Coffee Mug", Price = 12.5m }, ct);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<Product>(ct);

        var first = await _client.GetAsync($"/products/{created!.Id}", ct);
        var second = await _client.GetAsync($"/products/{created.Id}", ct);

        first.Headers.GetValues("X-Cache").ShouldBe(["MISS"]);
        second.Headers.GetValues("X-Cache").ShouldBe(["HIT"]);
        (await second.Content.ReadFromJsonAsync<Product>(ct)).ShouldBe(created);
    }
}
