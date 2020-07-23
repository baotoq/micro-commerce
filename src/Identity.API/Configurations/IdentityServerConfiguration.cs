using System.Collections.Generic;
using System.Linq;
using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;

namespace Identity.API.Configurations
{
    public static class IdentityServerConfiguration
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityConstants.IdentityResources.Roles()
            };

        public static IEnumerable<ApiResource> ApiResources =>
            new[]
            {
                new ApiResource(IdentityConstants.ApiResource.CatalogApi) { ApiSecrets = { new Secret("secret".Sha256()) } },
                new ApiResource(IdentityServerConstants.LocalApi.ScopeName),
            };

        public static IEnumerable<Client> Clients(IConfiguration configuration) =>
            new[]
            {
                new Client
                {
                    ClientId = "swagger",
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AllowedGrantTypes = GrantTypes.Code,

                    RequireConsent = false,
                    RequirePkce = true,

                    RedirectUris = configuration.GetSection("Client:Swagger:Uri").Get<string[]>().Select(s => s + "/oauth2-redirect.html").ToList(),
                    PostLogoutRedirectUris = configuration.GetSection("Client:Swagger:Uri").Get<string[]>().Select(s => s + "/oauth2-redirect.html").ToList(),
                    AllowedCorsOrigins = configuration.GetSection("Client:Swagger:Uri").Get<string[]>(),

                    AllowedScopes = new List<string>
                    {
                        IdentityConstants.ApiResource.CatalogApi,
                        IdentityServerConstants.LocalApi.ScopeName
                    }
                },
                new Client
                {
                    ClientName = "react-web",
                    ClientId = "react-web",
                    AccessTokenType = AccessTokenType.Jwt,
                    AllowedGrantTypes = GrantTypes.Code,
                    AllowAccessTokensViaBrowser = true,

                    RequireClientSecret = false,
                    RequireConsent = false,
                    RequirePkce = true,

                    RedirectUris =
                    {
                        $"{configuration["Client:React:Uri"]}/authentication/login-callback",
                        $"{configuration["Client:React:Uri"]}/silent-renew.html",
                        $"{configuration["Client:React:Uri"]}",
                    },
                    PostLogoutRedirectUris =
                    {
                        $"{configuration["Client:React:Uri"]}/unauthorized",
                        $"{configuration["Client:React:Uri"]}/authentication/logout-callback",
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
                        IdentityServerConstants.LocalApi.ScopeName,
                        IdentityConstants.IdentityResources.Roles.ScopeName,
                        IdentityConstants.ApiResource.CatalogApi
                    }
                }
            };
    }
}
