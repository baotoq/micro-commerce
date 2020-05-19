using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;

namespace Identity.API.Configurations
{
    public static class IdentityServerConfiguration
    {
        public static IEnumerable<IdentityResource> Ids =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };

        public static IEnumerable<ApiResource> Apis =>
            new[]
            {
                new ApiResource
                {
                    Name = "bshop-api",
                    ApiSecrets = { new Secret("secret".Sha256()) },
                    Scopes = { new Scope("bshop-api") }
                }
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

                    RedirectUris =           { $"{configuration["Client:Swagger:Uri"]}/oauth2-redirect.html" },
                    PostLogoutRedirectUris = { $"{configuration["Client:Swagger:Uri"]}/oauth2-redirect.html" },
                    AllowedCorsOrigins =     { $"{configuration["Client:Swagger:Uri"]}" },

                    AllowedScopes = new List<string>
                    {
                        "bshop-api"
                    }
                },
                new Client
                {
                    ClientName = "react-web",
                    ClientId = "react-web",
                    AccessTokenType = AccessTokenType.Reference,
                    AllowedGrantTypes = GrantTypes.Code,
                    AllowAccessTokensViaBrowser = true,

                    RequireClientSecret = false,
                    RequireConsent = false,
                    RequirePkce = true,

                    RedirectUris =
                    {
                        $"{configuration["Client:React:Uri"]}/authentication/login-callback",
                        $"{configuration["Client:React:Uri"]}/silent-renew.html",
                        $"{configuration["Client:React:Uri"]}"
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
                        "bshop-api"
                    }
                }
            };
    }
}