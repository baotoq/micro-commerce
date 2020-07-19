using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using UnitOfWork.Models;

namespace Identity.API.Data.Models
{
    public class Role : IdentityRole, IEntity<string>
    {
        public IList<UserRole> Users { get; set; } = new List<UserRole>();
    }
}
