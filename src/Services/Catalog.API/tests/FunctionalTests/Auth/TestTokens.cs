using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace MicroCommerce.Catalog.FunctionalTests.Auth;

/// <summary>
/// Mints self-signed JWTs for the FunctionalTests so the REAL JwtBearer handler and the
/// REAL seller policy validate them end-to-end — no Keycloak container, no back-channel
/// metadata fetch. <see cref="CatalogWebApplicationFactory"/> pre-seeds the matching
/// <see cref="TokenValidationParameters"/> (same key, issuer, audience, role-claim type).
/// </summary>
public static class TestTokens
{
    /// <summary>Issuer both the minter and the factory's validation parameters agree on.</summary>
    public const string Issuer = "https://test-issuer";

    /// <summary>Default audience the catalog-api validates (see plan §1/§2).</summary>
    public const string Audience = "catalog-api";

    /// <summary>
    /// Claim type the realm-role mapper flattens roles into; ASP.NET surfaces each array
    /// element as a role claim when <c>RoleClaimType = "roles"</c>.
    /// </summary>
    public const string RoleClaimType = "roles";

    // 256-bit secret shared with the factory's IssuerSigningKey. Test-only; never used in prod.
    private const string SigningSecret = "micro-commerce-functional-tests-symmetric-signing-key-256bit!!";

    /// <summary>The symmetric key shared between the minter and the JwtBearer validation parameters.</summary>
    public static readonly SymmetricSecurityKey SigningKey =
        new(Encoding.UTF8.GetBytes(SigningSecret));

    public static string Mint(string[] roles, string audience = Audience)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, "test-seller"),
            new(JwtRegisteredClaimNames.PreferredUsername, "seller@microcommerce.dev"),
        };
        // Flattened multivalued "roles" claim, mirroring the oidc-usermodel-realm-role-mapper.
        claims.AddRange(roles.Select(r => new Claim(RoleClaimType, r)));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = audience,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = DateTime.UtcNow,
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256),
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
