using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using MicroCommerce.ApiService.Features.Accounts;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace MicroCommerce.FunctionalTests;

/// <summary>
/// A person the tests act as. Each call to a factory method gives a fresh token subject,
/// so tests never collide with Accounts created by other tests in the shared database.
/// </summary>
public sealed record TestActor(string Subject, string Email, string Name, bool IsPlatformOperator = false)
{
    public static TestActor Buyer() => Create("buyer");

    public static TestActor PlatformOperator() => Create("operator") with { IsPlatformOperator = true };

    private static TestActor Create(string kind)
    {
        var subject = Guid.NewGuid().ToString();
        return new(subject, $"{kind}-{subject}@micro-commerce.test", $"Test {kind} {subject[..8]}");
    }
}

/// <summary>
/// Stands in for Keycloak: <see cref="ApiFixture"/> makes the API trust <see cref="SigningKey"/>,
/// and tokens minted here carry the same claims the dev realm puts in its access tokens.
/// </summary>
public static class TestIdentity
{
    public const string Issuer = "https://identity.micro-commerce.test/realms/micro-commerce";
    public const string Audience = "micro-commerce-api";

    public static readonly SecurityKey SigningKey = NewKey();

    public static SecurityKey NewKey() => new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32));

    public static string TokenFor(
        TestActor actor,
        DateTime? expires = null,
        SecurityKey? signingKey = null,
        string audience = Audience)
    {
        var claims = new List<Claim>
        {
            new("sub", actor.Subject),
            new("email", actor.Email),
            new("name", actor.Name),
            new("preferred_username", actor.Email),
        };
        if (actor.IsPlatformOperator) claims.Add(new("roles", AuthPolicies.PlatformOperatorRole));

        var expiresAt = expires ?? DateTime.UtcNow.AddMinutes(5);
        var issuedAt = expiresAt.AddMinutes(-5);

        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = audience,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = issuedAt,
            NotBefore = issuedAt,
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(signingKey ?? SigningKey, SecurityAlgorithms.HmacSha256),
        });
    }

    public static void Authenticate(this HttpClient client, string token) =>
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
}
