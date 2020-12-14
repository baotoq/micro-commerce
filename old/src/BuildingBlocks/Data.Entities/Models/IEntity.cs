namespace Data.Entities.Models
{
    public interface IEntity<out TId>
    {
        TId Id { get; }
    }
}
