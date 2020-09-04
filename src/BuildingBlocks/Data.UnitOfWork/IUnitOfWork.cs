using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Data.Entities.Models;

namespace Data.UnitOfWork
{
    public interface IUnitOfWork
    {
        IRepository<TEntity> Repository<TEntity>() where TEntity : IEntity<long>;
        IRepository<TEntity, TId> Repository<TEntity, TId>() where TEntity : IEntity<TId>;
        DbConnection Connection { get; }
        DbTransaction Transaction { get; }
        void Commit();
        Task CommitAsync(CancellationToken cancellationToken = default);
    }
}
