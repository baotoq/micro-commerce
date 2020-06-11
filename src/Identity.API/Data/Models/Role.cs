using Microsoft.AspNetCore.Identity;
using UnitOfWork.Models;

namespace Identity.API.Data.Models
{
    public class Role : IdentityRole<long>, IEntity<long>
    {
    }
}
