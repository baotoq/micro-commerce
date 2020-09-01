using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Data.Entities.Models;

namespace Data.UnitOfWork
{
    public class Repository<TEntity> : Repository<TEntity, long>, IRepository<TEntity> where TEntity : class, IEntity<long>
    {
        public Repository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }

    public class Repository<TEntity, TId> : IRepository<TEntity, TId> where TEntity : class, IEntity<TId>
    {
        protected IDbTransaction Transaction { get; }
        protected IDbConnection Connection => Transaction.Connection;

        public Repository(IUnitOfWork unitOfWork)
        {
            Transaction = unitOfWork.Transaction;
        }

        public IQueryable<TEntity> Query()
        {
            throw new System.NotImplementedException();
        }

        public async ValueTask<TEntity> FindAsync(TId id, CancellationToken cancellationToken = default) => await Connection.GetAsync<TEntity>(id, Transaction);

        public void Add(TEntity entity) => Connection.Insert<TId, TEntity>(entity, Transaction);

        public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) => Connection.InsertAsync<TId, TEntity>(entity, Transaction);

        public void Remove(TEntity entity)
        {
            Connection.Delete(entity);
        }
    }
}
