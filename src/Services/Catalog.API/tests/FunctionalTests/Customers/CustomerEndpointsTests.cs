using System.Net;
using System.Net.Http.Json;
using MicroCommerce.Catalog.Application.Customers.Commands;
using MicroCommerce.Catalog.Application.Customers.Dtos;

namespace MicroCommerce.Catalog.FunctionalTests.Customers;

public class CustomerEndpointsTests(CatalogWebApplicationFactory factory) : IClassFixture<CatalogWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly SpyOutputCacheStore _cache = (SpyOutputCacheStore)factory.Services.GetService(typeof(SpyOutputCacheStore))!;

    private static string NewEmail() => $"fn-{Guid.NewGuid():N}@example.com";

    private static CreateCustomerCommand NewCustomerCommand(string email) =>
        new("Test Buyer", email, "San Francisco, CA");

    private sealed record AddTagBody(string Tag);

    [Fact]
    public async Task GetCustomers_Returns200()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await _client.GetAsync("/api/customers?page=1&limit=100", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paged = await response.Content.ReadFromJsonAsync<PagedCustomers>(ct);
        Assert.NotNull(paged);
    }

    [Fact]
    public async Task CreateCustomer_Valid_Returns201WithLocation()
    {
        var email = NewEmail();
        var ct = TestContext.Current.CancellationToken;

        var response = await _client.PostAsJsonAsync("/api/customers", NewCustomerCommand(email), ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var created = await response.Content.ReadFromJsonAsync<CustomerDto>(ct);
        Assert.NotNull(created);
        Assert.Equal(email, created!.Email);
    }

    [Fact]
    public async Task CreateCustomer_DuplicateEmail_Returns409()
    {
        var email = NewEmail();
        var ct = TestContext.Current.CancellationToken;
        await _client.PostAsJsonAsync("/api/customers", NewCustomerCommand(email), ct);

        var response = await _client.PostAsJsonAsync("/api/customers", NewCustomerCommand(email), ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomerByEmail_Unknown_Returns404()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await _client.GetAsync($"/api/customers/{NewEmail()}", ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddTag_UnknownEmail_Returns404()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await _client.PostAsJsonAsync($"/api/customers/{NewEmail()}/tags", new AddTagBody("VIP"), ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddTag_Valid_Returns200_AndEvictsTag()
    {
        var email = NewEmail();
        var ct = TestContext.Current.CancellationToken;
        await _client.PostAsJsonAsync("/api/customers", NewCustomerCommand(email), ct);

        var beforeCount = _cache.EvictedTags.Count(t => t == "customers");

        var response = await _client.PostAsJsonAsync($"/api/customers/{email}/tags", new AddTagBody("VIP"), ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<CustomerDto>(ct);
        Assert.NotNull(updated);
        Assert.Contains("VIP", updated!.Tags);

        Assert.True(_cache.EvictedTags.Count(t => t == "customers") > beforeCount,
            "Expected EvictByTagAsync(\"customers\", ...) after a successful tag add.");
    }

    private sealed record PagedCustomers(IReadOnlyList<CustomerDto> Items, int Total, int Page, int PageSize);
}
