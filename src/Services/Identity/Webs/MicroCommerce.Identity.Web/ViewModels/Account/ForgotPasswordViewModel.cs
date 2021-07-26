using System.ComponentModel.DataAnnotations;
using Skoruba.IdentityServer4.Shared.Configuration.Configuration.Identity;

namespace MicroCommerce.Identity.STS.Identity.ViewModels.Account
{
    public class ForgotPasswordViewModel
    {
        [Required]
        public LoginResolutionPolicy? Policy { get; set; }
        
        [EmailAddress]
        public string Email { get; set; }

        public string Username { get; set; }
    }
}








