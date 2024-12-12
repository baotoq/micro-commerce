using MicroCommerce.ApiService.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace MicroCommerce.ApiService.Domain.Entities;

public class User : IdentityUser<Guid>, IDateEntity
{
    public sealed override Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public User()
    {
        CreatedAt = DateTimeOffset.UtcNow;
        Id = Guid.CreateVersion7(CreatedAt);
    }
}
