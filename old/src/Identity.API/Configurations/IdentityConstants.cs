using System.Collections.Generic;
using IdentityModel;
using IdentityServer4.Models;

namespace Identity.API.Configurations
{
    public static class IdentityConstants
    {
        public static class ApiResource
        {
            public const string CatalogApi = "catalog-api";
        }

        public class IdentityResources
        {
            public class Roles : IdentityResource
            {
                public static string ScopeName { get; } = "roles";

                public Roles() : base(ScopeName, new List<string> { JwtClaimTypes.Role })
                {
                }
            }
        }
    }
}
