using System.Collections.Generic;
using System.Linq;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;

namespace MicroCommerce.Identity.API.Configuration
{
    public static class IdentityServerConfiguration
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Email(),
                new IdentityResourceProfileWithRole()
            };

        public static IEnumerable<ApiResource> ApiResources =>
            new[]
            {
                new ApiResource(IdentityConstants.ApiResource.BasketApi) { Scopes = new[] { IdentityConstants.ApiResource.BasketApi }, UserClaims = new[] { JwtClaimTypes.Role, JwtClaimTypes.Name } },
                new ApiResource(IdentityConstants.ApiResource.CatalogApi) { Scopes = new[] { IdentityConstants.ApiResource.CatalogApi }, UserClaims = new[] { JwtClaimTypes.Role } },
                new ApiResource(IdentityConstants.ApiResource.OrderingApi) { Scopes = new[] { IdentityConstants.ApiResource.OrderingApi } },
                new ApiResource(IdentityServerConstants.LocalApi.ScopeName) { Scopes = new[] { IdentityServerConstants.LocalApi.ScopeName }, UserClaims = new[] { JwtClaimTypes.Role }  },
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new[]
            {
                new ApiScope(IdentityConstants.ApiResource.BasketApi),
                new ApiScope(IdentityConstants.ApiResource.CatalogApi),
                new ApiScope(IdentityConstants.ApiResource.OrderingApi),
                new ApiScope(IdentityServerConstants.LocalApi.ScopeName)
            };

        public static IEnumerable<Client> Clients(IConfiguration configuration) =>
           new[]
           {
                new Client
                {
                    ClientId = "swagger",
                    ClientName = "swagger",
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AccessTokenType = AccessTokenType.Jwt,
                    AllowedGrantTypes = GrantTypes.Code,

                    RequireConsent = false,
                    RequirePkce = true,

                    RedirectUris = configuration.GetSection("Client:Swagger:Uri").Get<string[]>().Select(s => s + "/swagger/oauth2-redirect.html").ToList(),
                    PostLogoutRedirectUris = configuration.GetSection("Client:Swagger:Uri").Get<string[]>().Select(s => s + "/swagger/oauth2-redirect.html").ToList(),
                    AllowedCorsOrigins = configuration.GetSection("Client:Swagger:Uri").Get<string[]>(),

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.LocalApi.ScopeName,
                        IdentityConstants.ApiResource.BasketApi,
                        IdentityConstants.ApiResource.CatalogApi,
                        IdentityConstants.ApiResource.OrderingApi,
                    },
                },
                new Client
                {
                    ClientId = "react-web",
                    ClientName = "react-web",
                    ClientSecrets =  { new Secret("secret".Sha256()) },
                    AccessTokenType = AccessTokenType.Jwt,
                    AllowedGrantTypes = GrantTypes.Code,
                    AllowOfflineAccess = true,

                    RequireConsent = false,
                    RequirePkce = false,

                    RedirectUris =
                    {
                        $"{configuration["Client:React:Uri"]}/api/auth/callback/identity-server4",
                        $"{configuration["Client:React:Uri"]}/auth/login-callback",
                        $"{configuration["Client:React:Uri"]}/silent-renew.html",
                        $"{configuration["Client:React:Uri"]}",
                    },
                    PostLogoutRedirectUris =
                    {
                        $"{configuration["Client:React:Uri"]}/auth/unauthorized",
                        $"{configuration["Client:React:Uri"]}/auth/logout-callback",
                        $"{configuration["Client:React:Uri"]}"
                    },
                    AllowedCorsOrigins =
                    {
                        $"{configuration["Client:React:Uri"]}"
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.LocalApi.ScopeName,
                        IdentityConstants.ApiResource.BasketApi,
                        IdentityConstants.ApiResource.CatalogApi,
                        IdentityConstants.ApiResource.OrderingApi,
                    }
                },
                new Client
                {
                    ClientId = "angular-web",
                    ClientName = "angular-web",
                    AccessTokenType = AccessTokenType.Jwt,
                    AllowedGrantTypes = GrantTypes.Code,
                    AllowOfflineAccess = true,

                    RequireClientSecret = false,
                    RequireConsent = false,
                    RequirePkce = true,

                    RedirectUris =
                    {
                        $"{configuration["Client:Angular:Uri"]}/silent-renew.html",
                        $"{configuration["Client:Angular:Uri"]}/index.html",
                        $"{configuration["Client:Angular:Uri"]}",
                    },
                    PostLogoutRedirectUris =
                    {
                        $"{configuration["Client:Angular:Uri"]}"
                    },
                    AllowedCorsOrigins =
                    {
                        $"{configuration["Client:Angular:Uri"]}"
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.LocalApi.ScopeName,
                        IdentityConstants.ApiResource.BasketApi,
                        IdentityConstants.ApiResource.CatalogApi,
                        IdentityConstants.ApiResource.OrderingApi,
                    }
                }
           };
    }
}
