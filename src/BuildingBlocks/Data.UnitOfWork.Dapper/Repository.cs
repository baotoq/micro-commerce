using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Data.Entities.Models;
using Data.UnitOfWork.Dapper.Core;

namespace Data.UnitOfWork.Dapper
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

        public virtual async ValueTask<TEntity> FindAsync(TId id, CancellationToken cancellationToken = default) => await Uow.Connection.GetAsync<TEntity>(id);

        public virtual Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) => Uow.Connection.InsertAsync<TId, TEntity>(entity);

        public virtual Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default) => Uow.Connection.DeleteAsync(entity);
    }
}
