using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace BShop.API.Data
{
    public interface IRepository<T> where T : Entity
    {
        IQueryable<T> Query();

        ValueTask<T> FindAsync(CancellationToken cancellationToken = default, params object[] keyValues);

        void Add(T entity);

        Task AddAsync(T entity, CancellationToken cancellationToken = default);

        void AddRange(IEnumerable<T> entity);

        void Remove(T entity);

        IDbContextTransaction BeginTransaction();

        void SaveChanges();

        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
