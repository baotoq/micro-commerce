using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.UnitOfWork.EF
{
    public class Repository<TEntity> : Repository<TEntity, long>, IRepository<TEntity> where TEntity : class, IEntity<long>
    {
        public Repository(DbContext context) : base(context)
        {
        }
    }

    public class Repository<TEntity, TId> : IRepository<TEntity, TId> where TEntity : class, IEntity<TId>
    {
        protected DbContext Context { get; }
        protected DbSet<TEntity> DbSet { get; }

        public Repository(DbContext context)
        {
            Context = context;
            DbSet = Context.Set<TEntity>();
        }

        public IQueryable<TEntity> Query() => DbSet;

        public ValueTask<TEntity> FindAsync(TId id, CancellationToken cancellationToken = default) => DbSet.FindAsync(new object[] { id }, cancellationToken);

        public void Add(TEntity entity) => DbSet.Add(entity);

        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) => await DbSet.AddAsync(entity, cancellationToken);

        public void AddRange(IEnumerable<TEntity> entity) => DbSet.AddRange(entity);

        public void Remove(TEntity entity) => DbSet.Remove(entity);
    }
}
