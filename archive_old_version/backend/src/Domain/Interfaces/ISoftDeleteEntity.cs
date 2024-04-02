namespace Domain.Interfaces;

public interface ISoftDeleteEntity
{
    public DateTimeOffset? DeletedAt { get; set; }
}