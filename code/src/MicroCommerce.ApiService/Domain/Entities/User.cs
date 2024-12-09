using Microsoft.AspNetCore.Identity;

namespace MicroCommerce.ApiService.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public override Guid Id { get; set; } = Guid.CreateVersion7();
}
