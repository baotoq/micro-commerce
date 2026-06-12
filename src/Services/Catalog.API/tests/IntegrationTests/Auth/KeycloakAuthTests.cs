using System.Net;
using System.Net.Http.Json;
using MicroCommerce.Catalog.Application.Promotions.Commands;
using MicroCommerce.Catalog.Application.Promotions.Dtos;
using MicroCommerce.Catalog.IntegrationTests;

namespace MicroCommerce.Catalog.IntegrationTests.Auth;

/// <summary>
/// The single Aspire IntegrationTest that pays the real Keycloak startup cost (plan §5).
/// Boots the full Testing-env app graph (keycloak is declared OUTSIDE the Testing guard in
/// AppHost.cs), obtains a real access token for the seeded dev seller via the Keycloak
/// password grant (directAccessGrantsEnabled=true), and exercises a representative seller
/// write end to end against the live JwtBearer + seller policy pipeline.
/// </summary>
[Collection("Api")]
public class KeycloakAuthTests(ApiFixture fixture)
{
    private const string Realm = "microcommerce";
    private const string ClientId = "microcommerce-web";
    private const string ClientSecret = "dev-only-web-secret";
    private const string SellerUsername = "seller@microcommerce.dev";
    private const string SellerPassword = "Passw0rd!";

    private readonly HttpClient _client = fixture.HttpClient;
    private readonly HttpClient _keycloak = fixture.KeycloakHttpClient;

    private static string NewCode() => $"KC-{Guid.NewGuid():N}"[..16].ToUpperInvariant();

    // Mirrors the FunctionalTests PromotionEndpointsTests percentage body shape.
    private static CreatePromotionCommand NewPercentageCommand(string code) =>
        new(code, "sitewide", "percentage", 20, null, null, null, null, false);

    private async Task<string> GetSellerAccessTokenAsync(CancellationToken ct)
    {
        // Resource Owner Password Credentials grant — enabled on the realm's web client so
        // tests can mint a real token without driving the browser login form.
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = ClientId,
            ["client_secret"] = ClientSecret,
            ["username"] = SellerUsername,
            ["password"] = SellerPassword,
            ["scope"] = "openid",
        });

        // BaseAddress is the keycloak resource endpoint resolved from the app model
        // (same mechanism the fixture uses for catalog-api).
        var response = await _keycloak.PostAsync(
            $"/realms/{Realm}/protocol/openid-connect/token", form, ct);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<TokenResponse>(ct);
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload!.AccessToken));
        return payload.AccessToken!;
    }

    [Fact]
    public async Task CreatePromotion_WithSellerBearerToken_Returns201()
    {
        var ct = TestContext.Current.CancellationToken;
        var token = await GetSellerAccessTokenAsync(ct);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/promotions")
        {
            Content = JsonContent.Create(NewPercentageCommand(NewCode())),
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request, ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<PromotionDto>(ct);
        Assert.NotNull(created);
        Assert.Equal("percentage", created!.Kind);
        Assert.Equal("draft", created.Status);
    }

    [Fact]
    public async Task CreatePromotion_WithoutToken_Returns401()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await _client.PostAsJsonAsync("/api/promotions", NewPercentageCommand(NewCode()), ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private sealed record TokenResponse(
        [property: System.Text.Json.Serialization.JsonPropertyName("access_token")] string? AccessToken);
}
