using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data.Entities.Models;

namespace Data.UnitOfWork.EF.Core
{
    public interface IRepository<T> : IRepository<T, long> where T : IEntity<long>
    {
    }

    public interface IRepository<T, in TId> : IQueryRepository<T, TId>, ICommandRepository<T, TId> where T : IEntity<TId>
    {
        IQueryable<T> Query();
    }

    public interface IQueryRepository<T, in TId> where T : IEntity<TId>
    {
        ValueTask<T> FindAsync(TId id, CancellationToken cancellationToken = default);
    }

    public interface ICommandRepository<in T, in TId> where T : IEntity<TId>
    {
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        Task RemoveAsync(T entity, CancellationToken cancellationToken = default);
    }
}
