using System.Data.Common;
using Data.Entities.Models;

namespace Data.UnitOfWork.Dapper.Core
{
    public interface IUnitOfWork
    {
        IRepository<TEntity> Repository<TEntity>() where TEntity : IEntity<long>;
        IRepository<TEntity, TId> Repository<TEntity, TId>() where TEntity : IEntity<TId>;
        DbConnection Connection { get; }
    }
}
