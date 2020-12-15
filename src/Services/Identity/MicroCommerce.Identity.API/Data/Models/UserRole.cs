using Microsoft.AspNetCore.Identity;

namespace MicroCommerce.Identity.API.Data.Models
{
    public class UserRole : IdentityUserRole<string>
    {
        public User User { get; set; }

        public Role Role { get; set; }
    }
}
