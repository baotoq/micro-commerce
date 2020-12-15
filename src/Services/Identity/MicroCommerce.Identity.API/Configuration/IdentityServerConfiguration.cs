using System.Collections.Generic;
using System.Linq;
using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;

namespace MicroCommerce.Identity.API.Configuration
{
    public class IdentityServerConfiguration
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };

        public static IEnumerable<ApiResource> ApiResources =>
            new[]
            {
                new ApiResource(IdentityConstants.ApiResource.BasketApi),
                new ApiResource(IdentityConstants.ApiResource.CatalogApi),
                new ApiResource(IdentityConstants.ApiResource.OrderingApi),
                new ApiResource(IdentityServerConstants.LocalApi.ScopeName),
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
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AllowedGrantTypes = GrantTypes.Code,

                    RequireConsent = false,
                    RequirePkce = true,

                    RedirectUris = configuration.GetSection("Client:Swagger:Uri").Get<string[]>().Select(s => s + "/swagger/oauth2-redirect.html").ToList(),
                    PostLogoutRedirectUris = configuration.GetSection("Client:Swagger:Uri").Get<string[]>().Select(s => s + "/swagger/oauth2-redirect.html").ToList(),
                    AllowedCorsOrigins = configuration.GetSection("Client:Swagger:Uri").Get<string[]>(),

                    AllowedScopes = new List<string>
                    {
                        IdentityConstants.ApiResource.BasketApi,
                        IdentityConstants.ApiResource.CatalogApi,
                        IdentityConstants.ApiResource.OrderingApi,
                        IdentityServerConstants.LocalApi.ScopeName
                    }
                }
           };
    }
}
