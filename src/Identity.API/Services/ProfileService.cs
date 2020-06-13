using System.Threading.Tasks;
using Identity.API.Data.Models;
using IdentityServer4.AspNetIdentity;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Identity.API.Services
{
    public class ProfileService : ProfileService<User>
    {
        public ProfileService(UserManager<User> userManager, IUserClaimsPrincipalFactory<User> claimsFactory, ILogger<ProfileService<User>> logger) : base(userManager, claimsFactory, logger)
        {
        }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            context.LogProfileRequest(Logger);
            await base.GetProfileDataAsync(context);
            context.LogIssuedClaims(Logger);
        }
    }
}
