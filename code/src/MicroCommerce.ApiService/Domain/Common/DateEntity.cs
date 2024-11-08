namespace MicroCommerce.ApiService.Domain.Common;

public abstract class DateEntity
{
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}
