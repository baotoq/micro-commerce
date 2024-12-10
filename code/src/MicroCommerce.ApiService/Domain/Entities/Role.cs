using Microsoft.AspNetCore.Identity;

namespace MicroCommerce.ApiService.Domain.Entities;

public class Role : IdentityRole<Guid>
{
    public override Guid Id { get; set; } = Guid.CreateVersion7();
}
