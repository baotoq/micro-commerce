using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MicroCommerce.IntegrationTests;

/// <summary>A real Keycloak token for a seeded Account is accepted by <c>GET /api/me</c> (FR-007 to FR-009).</summary>
public class MeApiTests(AppHostFixture fixture)
{
    private sealed record Me(Guid AccountId, string DisplayName, string? Email, bool IsPlatformOperator);

    [Fact]
    public async Task Seeded_buyer_signs_in_and_gets_their_account()
    {
        var ct = TestContext.Current.CancellationToken;

        var me = await GetMeAsync(await fixture.BuyerTokenAsync(ct), ct);

        me.DisplayName.ShouldBe("Bea Buyer");
        me.Email.ShouldBe("buyer@micro-commerce.test");
        me.AccountId.ShouldNotBe(Guid.Empty);
        me.IsPlatformOperator.ShouldBeFalse();
    }

    [Fact]
    public async Task Seeded_operator_is_a_platform_operator()
    {
        var ct = TestContext.Current.CancellationToken;

        var me = await GetMeAsync(await fixture.OperatorTokenAsync(ct), ct);

        me.DisplayName.ShouldBe("Olive Operator");
        me.IsPlatformOperator.ShouldBeTrue();
    }

    [Fact]
    public async Task Me_without_a_token_is_unauthorized()
    {
        var response = await fixture.ApiClient.GetAsync("/api/me", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private async Task<Me> GetMeAsync(string token, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await fixture.ApiClient.SendAsync(request, ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<Me>(ct))!;
    }
}
