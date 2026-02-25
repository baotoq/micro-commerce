using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MicroCommerce.ApiService.Tests.Integration.Fixtures;

/// <summary>
/// Fake authentication handler that bypasses Keycloak for integration tests.
/// Reads the X-Test-UserId header and injects it as NameIdentifier and sub claims.
/// Tests control identity by setting this header on their HttpClient.
/// </summary>
public sealed class FakeAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check for test userId header set by CreateAuthenticatedClient
        if (!Request.Headers.TryGetValue("X-Test-UserId", out Microsoft.Extensions.Primitives.StringValues userIdValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        string userId = userIdValues.ToString();

        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("sub", userId),
        ];

        ClaimsIdentity identity = new(claims, SchemeName);
        ClaimsPrincipal principal = new(identity);
        AuthenticationTicket ticket = new(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
