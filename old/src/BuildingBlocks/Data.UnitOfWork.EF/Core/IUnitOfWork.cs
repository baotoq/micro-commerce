using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Data.Entities.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Data.UnitOfWork.EF.Core
{
    public interface IUnitOfWork
    {
        IRepository<TEntity> Repository<TEntity>() where TEntity : IEntity<long>;
        IRepository<TEntity, TId> Repository<TEntity, TId>() where TEntity : IEntity<TId>;
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default);
        void SaveChanges();
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
