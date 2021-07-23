using IdentityModel;
using IdentityServer4.Models;

namespace MicroCommerce.Identity.API.Configuration
{
    public class IdentityResourceProfileWithRole : IdentityResources.Profile
    {
        public IdentityResourceProfileWithRole()
        {
            UserClaims.Add(JwtClaimTypes.Role);
        }
    }
}
