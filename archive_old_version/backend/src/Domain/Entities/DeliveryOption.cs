using Domain.Interfaces;

namespace Domain.Entities;

public class DeliveryOption : EntityBase, IDateEntity, ISoftDeleteEntity
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal MinimumSpending { get; set; }
    public decimal Fee { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}