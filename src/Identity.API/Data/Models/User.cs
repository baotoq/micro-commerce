using Microsoft.AspNetCore.Identity;
using UnitOfWork.Models;

namespace Identity.API.Data.Models
{
    public class User : IdentityUser<long>, IEntity<long>
    {
    }
}
