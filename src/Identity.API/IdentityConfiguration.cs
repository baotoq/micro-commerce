using System.Collections.Generic;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace Identity.API
{
    public static class IdentityConfiguration
    {
        public static IEnumerable<IdentityResource> Ids => new IdentityResource[]
        {
            new IdentityResources.OpenId()
        };

        public static IEnumerable<ApiResource> ApiResources => new[]
        {
            new ApiResource("catalog-api", "Catalog API")
        };

        public static IEnumerable<Client> Clients => new[]
        {
            new Client
            {
                ClientId = "client",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },
                AllowedScopes = { "catalog-api" }
            }
        };

        public static List<TestUser> TestUsers => new List<TestUser>
        {
            new TestUser
            {
                SubjectId = "1",
                Username = "alice",
                Password = "password"
            },
            new TestUser
            {
                SubjectId = "2",
                Username = "bob",
                Password = "password"
            }
        };
    }
}