using System.Collections.Generic;
using Data.Entities.Models;
using Microsoft.AspNetCore.Identity;

namespace Identity.API.Data.Models
{
    public class User : IdentityUser, IEntity<string>
    {
        public IList<UserRole> Roles { get; set; } = new List<UserRole>();
    }
}
