namespace Domain.Interfaces;

public interface IDateEntity
{
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}