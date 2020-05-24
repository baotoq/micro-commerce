using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BShop.API.Data.Models.Common;

namespace BShop.API.Data
{
    public interface IRepository<T> where T : Entity
    {
        IQueryable<T> Query();

        ValueTask<T?> FindAsync(long id, CancellationToken cancellationToken = default);

        void Add(T entity);

        Task AddAsync(T entity, CancellationToken cancellationToken = default);

        void AddRange(IEnumerable<T> entity);

        void Remove(T entity);
    }
}
