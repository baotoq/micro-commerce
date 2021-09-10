using Skoruba.IdentityServer4.Shared.Configuration.Configuration.Identity;

namespace MicroCommerce.Identity.STS.Identity.Helpers.Localization
{
    public static class LoginPolicyResolutionLocalizer
    {
        public static string GetUserNameLocalizationKey(LoginResolutionPolicy policy)
        {
            return policy switch
            {
                LoginResolutionPolicy.Username => "Username",
                LoginResolutionPolicy.Email => "Email",
                _ => "Username",
            };
        }
    }
}
