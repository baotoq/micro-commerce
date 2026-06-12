using System.Security.Claims;
using MicroCommerce.Catalog.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MicroCommerce.Catalog.FunctionalTests.Auth;

/// <summary>
/// Exercises the REAL "seller" policy registered by <see cref="AuthenticationExtensions.AddCatalogAuthentication"/>
/// directly against <see cref="IAuthorizationService"/> — no HTTP pipeline, no containers. Verifies the
/// policy denies anonymous users, denies authenticated-but-roleless users, and allows the "seller" role
/// surfaced under the "roles" claim type (matching the realm-role mapper flattening, plan §1).
/// </summary>
public class SellerPolicyTests
{
    private static (IAuthorizationService Service, AuthorizationPolicy Policy) Build()
    {
        // Boot the real registration path so the test binds to the production policy definition.
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = "Testing";
        builder.AddCatalogAuthentication();
        var app = builder.Build();

        var service = app.Services.GetRequiredService<IAuthorizationService>();
        var provider = app.Services.GetRequiredService<IAuthorizationPolicyProvider>();
        var policy = provider.GetPolicyAsync(AuthenticationExtensions.SellerPolicy).GetAwaiter().GetResult();
        Assert.NotNull(policy);
        return (service, policy!);
    }

    private static ClaimsPrincipal Anonymous() => new(new ClaimsIdentity());

    // Authenticated principal: identity must carry an AuthenticationType so IsAuthenticated == true.
    private static ClaimsPrincipal Authenticated(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, authenticationType: "TestBearer", nameType: "sub", roleType: TestTokens.RoleClaimType));

    [Fact]
    public async Task SellerPolicy_AnonymousUser_IsDenied()
    {
        var (service, policy) = Build();

        var result = await service.AuthorizeAsync(Anonymous(), resource: null, policy);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task SellerPolicy_AuthenticatedWithoutSellerRole_IsDenied()
    {
        var (service, policy) = Build();
        var principal = Authenticated(new Claim("sub", "someone"), new Claim(TestTokens.RoleClaimType, "buyer"));

        var result = await service.AuthorizeAsync(principal, resource: null, policy);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task SellerPolicy_AuthenticatedWithSellerRole_IsAllowed()
    {
        var (service, policy) = Build();
        var principal = Authenticated(new Claim("sub", "alex"), new Claim(TestTokens.RoleClaimType, "seller"));

        var result = await service.AuthorizeAsync(principal, resource: null, policy);

        Assert.True(result.Succeeded);
    }
}
