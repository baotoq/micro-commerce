using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace MicroCommerce.IntegrationTests;

public class MeApiTests(AppHostFixture fixture)
{
    private sealed record Me(Guid Id, string? Email, string DisplayName, bool IsPlatformOperator);

    private sealed record TokenResponse([property: JsonPropertyName("access_token")] string AccessToken);

    [Fact]
    public async Task Seeded_operator_signs_in_with_keycloak_and_gets_their_account()
    {
        var ct = TestContext.Current.CancellationToken;
        var token = await GetTokenAsync("operator", "operator", ct);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await fixture.ApiClient.SendAsync(request, ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var me = await response.Content.ReadFromJsonAsync<Me>(ct);
        me.ShouldNotBeNull();
        me.Email.ShouldBe("operator@micro-commerce.test");
        me.DisplayName.ShouldBe("Olive Operator");
        me.IsPlatformOperator.ShouldBeTrue();
    }

    [Fact]
    public async Task Me_without_a_token_is_unauthorized()
    {
        var response = await fixture.ApiClient.GetAsync("/me", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    // Password grant through the realm's test-only client; the frontend client doesn't allow it.
    private async Task<string> GetTokenAsync(string username, string password, CancellationToken ct)
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

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(ct));
        return (await response.Content.ReadFromJsonAsync<TokenResponse>(ct))!.AccessToken;
    }
}
