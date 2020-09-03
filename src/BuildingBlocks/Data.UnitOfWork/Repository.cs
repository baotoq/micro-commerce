using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Data.Entities.Models;

namespace Data.UnitOfWork
{
    public class Repository<TEntity> : Repository<TEntity, long>, IRepository<TEntity> where TEntity : class, IEntity<long>
    {
        public Repository(IUnitOfWork uow) : base(uow)
        {
        }
    }

    public class Repository<TEntity, TId> : IRepository<TEntity, TId> where TEntity : class, IEntity<TId>
    {
        protected readonly IUnitOfWork Uow;

        public Repository(IUnitOfWork uow)
        {
            Uow = uow;
        }

        public IQueryable<TEntity> Query() => Uow.Connection.GetList<TEntity>(true).AsQueryable();

        public async ValueTask<TEntity> FindAsync(TId id, CancellationToken cancellationToken = default) => await Uow.Connection.GetAsync<TEntity>(id);

        public void Add(TEntity entity) => Uow.Connection.Insert<TId, TEntity>(entity, Uow.Transaction);

        public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) => Uow.Connection.InsertAsync<TId, TEntity>(entity);

        public void Remove(TEntity entity) => Uow.Connection.Delete(entity);
    }
}
