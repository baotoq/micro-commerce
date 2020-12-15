using System.Collections.Generic;
using MicroCommerce.Shared.Entities;
using Microsoft.AspNetCore.Identity;

namespace MicroCommerce.Identity.API.Data.Models
{
    public class Role : IdentityRole, IEntity<string>
    {
        public IList<UserRole> Users { get; set; } = new List<UserRole>();
    }
}
