namespace UnitOfWork.Models
{
    public abstract class Entity : IEntity<long>
    {
        public virtual long Id { get; protected set; }
    }
}
