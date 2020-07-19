using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using UnitOfWork.Models;

namespace Identity.API.Data.Models
{
    public class User : IdentityUser, IEntity<string>
    {
        public IList<UserRole> Roles { get; set; } = new List<UserRole>();
    }
}
