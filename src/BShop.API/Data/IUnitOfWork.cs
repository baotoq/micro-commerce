using System.Threading;
using System.Threading.Tasks;
using BShop.API.Data.Models.Common;

namespace BShop.API.Data
{
    public interface IUnitOfWork
    {
        IRepository<TEntity> Repository<TEntity>() where TEntity : Entity;

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
