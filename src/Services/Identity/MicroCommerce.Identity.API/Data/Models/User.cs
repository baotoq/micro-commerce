using System.Collections.Generic;
using MicroCommerce.Shared.Entities;
using Microsoft.AspNetCore.Identity;

namespace MicroCommerce.Identity.API.Data.Models
{
    public class User : IdentityUser, IEntity<string>
    {
        public IList<UserRole> Roles { get; set; } = new List<UserRole>();
    }
}
