using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data.Entities.Models;
using Data.UnitOfWork.EF.Core;
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

        public virtual ValueTask<TEntity> FindAsync(TId id, CancellationToken cancellationToken = default) => DbSet.FindAsync(new object[] { id }, cancellationToken);

        public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) => await DbSet.AddAsync(entity, cancellationToken);

        public virtual Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            DbSet.Remove(entity);
            return Task.CompletedTask;
        }
    }
}
