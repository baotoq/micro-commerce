using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting;

namespace MicroCommerce.Catalog.Api;

/// <summary>
/// Wires JWT bearer authentication against Keycloak and the seller authorization policy for the
/// Catalog API. Keycloak is hosted as a plain Aspire container over HTTP, so this uses the standard
/// <c>Microsoft.AspNetCore.Authentication.JwtBearer</c> handler with the authority injected by the
/// AppHost via the <c>Keycloak:Authority</c> config key (http://localhost:8080/realms/microcommerce).
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>Authorization policy name protecting every Catalog write endpoint group.</summary>
    public const string SellerPolicy = "seller";

    public static TBuilder AddCatalogAuthentication<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Injected by the AppHost (http://localhost:8080/realms/microcommerce). The handler
                // fetches OIDC metadata + JWKS from {Authority}/.well-known/openid-configuration.
                options.Authority = builder.Configuration["Keycloak:Authority"];
                options.Audience = "catalog-api";
                // Keep JWT claim names verbatim. With the default MapInboundClaims=true the
                // handler rewrites the principal through the legacy claim-type map, which drops
                // the flattened "roles" role mapping (token validates fine, but RequireRole 403s).
                options.MapInboundClaims = false;
                // Roles are flattened into a top-level "roles" claim by the realm-role mapper,
                // so RequireRole("seller") works without parsing nested realm_access JSON.
                options.TokenValidationParameters.RoleClaimType = "roles";

                // Dev/Testing reach Keycloak over plain HTTP; relax the HTTPS-metadata requirement.
                // The FunctionalTests never hit Keycloak (self-minted JWTs via PostConfigure), so
                // this only affects a live dev stack. Production keeps HTTPS metadata enforced.
                if (builder.Environment.IsDevelopment() || builder.Environment.EnvironmentName == "Testing")
                {
                    options.RequireHttpsMetadata = false;
                }
            });

        builder.Services.AddAuthorization(options =>
            options.AddPolicy(SellerPolicy, policy =>
                policy.RequireAuthenticatedUser().RequireRole("seller")));

        return builder;
    }
}
