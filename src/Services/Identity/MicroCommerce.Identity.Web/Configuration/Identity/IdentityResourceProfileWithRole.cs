using IdentityModel;
using IdentityServer4.Models;

namespace MicroCommerce.Identity.Web.Configuration.Identity
{
    public class IdentityResourceProfileWithRole : IdentityResources.Profile
    {
        public IdentityResourceProfileWithRole()
        {
            UserClaims.Add(JwtClaimTypes.Role);
        }
    }
}
