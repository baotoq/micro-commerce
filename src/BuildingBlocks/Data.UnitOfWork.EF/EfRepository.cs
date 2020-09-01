using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.UnitOfWork.EF
{
    public abstract class EfRepository<TEntity> : EfRepository<TEntity, long>, IRepository<TEntity> where TEntity : class, IEntity<long>
    {
        protected EfRepository(DbContext context) : base(context)
        {
        }
    }

    public abstract class EfRepository<TEntity, TId> : IRepository<TEntity, TId> where TEntity : class, IEntity<TId>
    {
        protected DbContext Context { get; }
        protected DbSet<TEntity> DbSet { get; }

        protected EfRepository(DbContext context)
        {
            Context = context;
            DbSet = Context.Set<TEntity>();
        }

        public IQueryable<TEntity> Query() => DbSet;

        public ValueTask<TEntity> FindAsync(TId id, CancellationToken cancellationToken = default) => DbSet.FindAsync(new object[] { id }, cancellationToken);

        public void Add(TEntity entity) => DbSet.Add(entity);

        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) => await DbSet.AddAsync(entity, cancellationToken);

        public void Remove(TEntity entity) => DbSet.Remove(entity);
    }
}
