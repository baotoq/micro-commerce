namespace Data.Entities.Models
{
    public abstract class Entity : IEntity<long>
    {
        public long Id { get; protected set; }
    }
}
