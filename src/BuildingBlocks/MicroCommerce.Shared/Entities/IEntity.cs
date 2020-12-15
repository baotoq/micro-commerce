namespace MicroCommerce.Shared.Entities
{
    public interface IEntity<out TId>
    {
        TId Id { get; }
    }
}
