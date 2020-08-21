using System.Threading;
using System.Threading.Tasks;
using Data.Entities.Models;

namespace Data.UnitOfWork.EF
{
    public interface IEfUnitOfWork : IUnitOfWork
    {
        IRepository<TEntity> Repository<TEntity>() where TEntity : IEntity<long>;
        IRepository<TEntity, TId> Repository<TEntity, TId>() where TEntity : IEntity<TId>;

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
