using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Data.Entities.Models;

namespace Data.UnitOfWork
{
    public interface IUnitOfWork
    {
        IRepository<TEntity> Repository<TEntity>() where TEntity : IEntity<long>;
        IRepository<TEntity, TId> Repository<TEntity, TId>() where TEntity : IEntity<TId>;
        IDbConnection Connection { get; }
        IDbTransaction Transaction { get; }
        void Commit();
        Task CommitAsync(CancellationToken cancellationToken = default);
    }
}
