using Domain.Interfaces;

namespace Domain.Entities;

public class Shop : IDateEntity
{
    public string Id { get; set; } = "";

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}