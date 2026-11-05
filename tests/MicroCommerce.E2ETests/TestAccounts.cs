using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace MicroCommerce.E2ETests;

/// <summary>
/// An Account a test signs in with. <see cref="ToString"/> returns only the username, so the
/// password never reaches test output, assertion messages or reports (FR-009).
/// </summary>
public sealed record TestAccount(string Username, string Password, string DisplayName)
{
    public override string ToString() => Username;
}

/// <summary>
/// The seeded development Accounts, and fresh Accounts created per test through the Keycloak admin
/// REST API (research R5, data-model "Test Account"). The API turns a token's <c>sub</c> into an
/// Account on first use, so a fresh Account needs no registration step.
/// </summary>
public sealed class TestAccounts(MarketplaceFixture fixture)
{
    private const string Realm = "micro-commerce";

    private readonly SemaphoreSlim _adminLock = new(1, 1);
    private string? _adminToken;

    private sealed record TokenResponse([property: JsonPropertyName("access_token")] string AccessToken);

    /// <summary>The seeded Buyer, "Bea Buyer". Owns no Merchant, and no test gives it one.</summary>
    public TestAccount Buyer => new("buyer", fixture.BuyerPassword, "Bea Buyer");

    /// <summary>The seeded Platform Operator, "Olive Operator".</summary>
    public TestAccount Operator => new("operator", fixture.OperatorPassword, "Olive Operator");

    /// <summary>
    /// Creates an Account that is fully set up, so Keycloak shows no "update profile" or "update
    /// password" page and the password grant works at once.
    /// </summary>
    public async Task<TestAccount> CreateAsync(string role, CancellationToken ct)
    {
        var username = $"e2e-{role}-{TestRun.Suffix}-{TestRun.Next()}".ToLowerInvariant();
        var password = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/admin/realms/{Realm}/users")
        {
            Content = JsonContent.Create(new
            {
                username,
                enabled = true,
                email = $"{username}@micro-commerce.test",
                emailVerified = true,
                firstName = "E2E",
                lastName = role,
                credentials = new[] { new { type = "password", value = password, temporary = false } },
                requiredActions = Array.Empty<string>(),
            }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await AdminTokenAsync(ct));

        var response = await fixture.KeycloakClient.SendAsync(request, ct);
        response.StatusCode.ShouldBe(HttpStatusCode.Created, $"Creating the test Account {username} failed.");
        return new TestAccount(username, password, $"E2E {role}");
    }

    /// <summary>A real access token for the Account, from the dev realm's test-only password-grant client.</summary>
    public async Task<string> TokenAsync(TestAccount account, CancellationToken ct)
    {
        var response = await fixture.KeycloakClient.PostAsync(
            $"/realms/{Realm}/protocol/openid-connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = "micro-commerce-tests",
                ["username"] = account.Username,
                ["password"] = account.Password,
                ["scope"] = "openid",
            }),
            ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, $"Getting a token for {account} failed.");
        return (await response.Content.ReadFromJsonAsync<TokenResponse>(ct))!.AccessToken;
    }

    private async Task<string> AdminTokenAsync(CancellationToken ct)
    {
        await _adminLock.WaitAsync(ct);
        try
        {
            if (_adminToken is not null) return _adminToken;

            var response = await fixture.KeycloakClient.PostAsync(
                "/realms/master/protocol/openid-connect/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["client_id"] = "admin-cli",
                    ["username"] = "admin",
                    ["password"] = fixture.KeycloakAdminPassword,
                }),
                ct);

            response.StatusCode.ShouldBe(HttpStatusCode.OK, "Getting the Keycloak admin token failed.");
            // Keycloak's admin tokens last 60 s by default; tests create Accounts early, and a later
            // expiry shows up as a 401 on CreateAsync, which would be fixed here, not retried.
            _adminToken = (await response.Content.ReadFromJsonAsync<TokenResponse>(ct))!.AccessToken;
            return _adminToken;
        }
        finally
        {
            _adminLock.Release();
        }
    }
}
