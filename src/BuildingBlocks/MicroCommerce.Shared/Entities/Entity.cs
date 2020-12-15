namespace MicroCommerce.Shared.Entities
{
    public abstract class Entity : IEntity<long>
    {
        public long Id { get; protected set; }
    }
}
