using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace MicroCommerce.IntegrationTests;

/// <summary>Gets real access tokens from the dev realm, through its test-only password-grant client.</summary>
public static class KeycloakTokens
{
    private sealed record TokenResponse([property: JsonPropertyName("access_token")] string AccessToken);

    public static Task<string> BuyerTokenAsync(this AppHostFixture fixture, CancellationToken ct) =>
        GetAsync(fixture, "buyer", fixture.BuyerPassword, ct);

    public static Task<string> OperatorTokenAsync(this AppHostFixture fixture, CancellationToken ct) =>
        GetAsync(fixture, "operator", fixture.OperatorPassword, ct);

    private static async Task<string> GetAsync(AppHostFixture fixture, string username, string password, CancellationToken ct)
    {
        var response = await fixture.KeycloakClient.PostAsync(
            "/realms/micro-commerce/protocol/openid-connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = "micro-commerce-tests",
                ["username"] = username,
                ["password"] = password,
                ["scope"] = "openid",
            }),
            ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<TokenResponse>(ct))!.AccessToken;
    }
}
