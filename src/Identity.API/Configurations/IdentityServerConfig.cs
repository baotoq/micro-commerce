using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using IdentityServer4;
using IdentityServer4.Models;

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
                new ApiResource("bshop-api", "BShop API")
            };

        public static IEnumerable<Client> Clients =>
            new[]
            {
                new Client
                {
                    ClientId = "bshop.ro",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AllowedScopes = { "bshop-api" }
                },
                new Client
                {
                    ClientName = "react_code_client",
                    ClientId = "react_code_client",
                    AccessTokenType = AccessTokenType.Reference,
                    AllowedGrantTypes = GrantTypes.Code,
                    AllowAccessTokensViaBrowser = true,

                    RequireClientSecret = false,
                    RequireConsent = false,
                    RequirePkce = true,

                    RedirectUris = new List<string>
                    {
                        $"http://localhost:3000/authentication/login-callback",
                        $"http://localhost:3000/silent-renew.html",
                        $"http://localhost:3000"
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        $"http://localhost:3000/unauthorized",
                        $"http://localhost:3000/authentication/logout-callback",
                        $"http://localhost:3000"
                    },
                    AllowedCorsOrigins = new List<string>
                    {
                        $"http://localhost:3000"
                    },
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "bshop-api"
                    }
                }
            };
    }
}