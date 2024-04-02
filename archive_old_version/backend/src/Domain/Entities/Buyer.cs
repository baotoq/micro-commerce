using Domain.Interfaces;

namespace Domain.Entities;

public class Buyer : IDateEntity
{
    public string Id { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}