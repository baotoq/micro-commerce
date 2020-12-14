using System.Collections.Generic;
using Data.Entities.Models;
using Microsoft.AspNetCore.Identity;

namespace Identity.API.Data.Models
{
    public class Role : IdentityRole, IEntity<string>
    {
        public IList<UserRole> Users { get; set; } = new List<UserRole>();
    }
}
