using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BShop.API.Data.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace BShop.API.Data
{
    public class Repository<T> : IRepository<T> where T : Entity
    {
        protected DbContext Context { get; }
        protected DbSet<T> DbSet { get; }

        public Repository(DbContext context)
        {
            Context = context;
            DbSet = Context.Set<T>();
        }

        public IQueryable<T> Query()
        {
            return DbSet;
        }

        public ValueTask<T?> FindAsync(long id, CancellationToken cancellationToken = default)
        {
            return DbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public void Add(T entity)
        {
            DbSet.Add(entity);
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await DbSet.AddAsync(entity, cancellationToken);
        }

        public void AddRange(IEnumerable<T> entity)
        {
            DbSet.AddRange(entity);
        }

        public void Remove(T entity)
        {
            DbSet.Remove(entity);
        }
    }
}
