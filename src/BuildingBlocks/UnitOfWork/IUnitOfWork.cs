using System.Threading;
using System.Threading.Tasks;
using UnitOfWork.Models;

namespace UnitOfWork
{
    public interface IUnitOfWork
    {
        IRepository<TEntity> Repository<TEntity>() where TEntity : IEntity<long>;

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
