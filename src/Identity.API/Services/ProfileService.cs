using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Identity.API.Data.Models;
using IdentityModel;
using IdentityServer4.AspNetIdentity;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Identity.API.Services
{
    public class ProfileService : IProfileService
    {
        protected readonly IUserClaimsPrincipalFactory<User> ClaimsFactory;
        protected readonly ILogger<ProfileService<User>> Logger;
        protected readonly UserManager<User> UserManager;

        public ProfileService(UserManager<User> userManager,
            IUserClaimsPrincipalFactory<User> claimsFactory,
            ILogger<ProfileService<User>> logger)
        {
            ClaimsFactory = claimsFactory;
            Logger = logger;
            UserManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject?.GetSubjectId();
            if (sub == null) throw new ArgumentException("No sub claim present");

            var user = await UserManager.FindByIdAsync(sub);
            if (user == null)
            {
                Logger?.LogWarning("No user found matching subject Id: {0}", sub);
            }
            else
            {
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtClaimTypes.Name, user.UserName),
                    new Claim(JwtClaimTypes.Email, user.Email)
                };

                var userRoles = await UserManager.GetRolesAsync(user);
                claims.AddRange(userRoles.Select(role => new Claim(JwtClaimTypes.Role, role)));

                context.IssuedClaims.AddRange(claims);
            }
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject?.GetSubjectId();
            if (sub == null) throw new ArgumentException("No subject Id claim present");

            var user = await UserManager.FindByIdAsync(sub);
            if (user == null)
            {
                Logger?.LogWarning("No user found matching subject Id: {0}", sub);
            }

            context.IsActive = user != null;
        }
    }
}
