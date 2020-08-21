using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data.Entities.Models;

namespace Data.UnitOfWork.EF
{
    public interface IRepository<T> : IRepository<T, long> where T : IEntity<long>
    {
    }

    public interface IRepository<T, in TId> where T : IEntity<TId>
    {
        IQueryable<T> Query();

        ValueTask<T> FindAsync(TId id, CancellationToken cancellationToken = default);

        void Add(T entity);

        Task AddAsync(T entity, CancellationToken cancellationToken = default);

        void AddRange(IEnumerable<T> entity);

        void Remove(T entity);
    }
}
