using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Data.Models.Common;

namespace Catalog.API.Data
{
    public interface IUnitOfWork
    {
        IRepository<TEntity> Repository<TEntity>() where TEntity : Entity;

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
