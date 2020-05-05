using System.Collections.Generic;
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
                new ApiResource("bshop", "BShop API")
            };

        public static IEnumerable<Client> Clients =>
            new[]
            {
                new Client
                {
                    ClientId = "bshop.ro.client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AllowedScopes = { "bshop-api" }
                }
            };
    }
}