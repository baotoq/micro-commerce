using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Data.Entities.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        protected readonly Func<IDbConnection> ConnFactoryFunc;
        protected IDbConnection DbConnection;
        protected IServiceProvider ServiceProvider { get; }

        public UnitOfWork(Func<IDbConnection> connFactoryFunc, IServiceProvider serviceProvider)
        {
            ConnFactoryFunc = connFactoryFunc;
            ServiceProvider = serviceProvider;
            Connection.Open();
            Transaction = Connection.BeginTransaction();
        }

        public IRepository<TEntity> Repository<TEntity>() where TEntity : IEntity<long> => ServiceProvider.GetRequiredService<IRepository<TEntity>>();

        public IRepository<TEntity, TId> Repository<TEntity, TId>() where TEntity : IEntity<TId> => ServiceProvider.GetRequiredService<IRepository<TEntity, TId>>();

        public IDbConnection Connection => DbConnection ??= ConnFactoryFunc();
        public IDbTransaction Transaction { get; private set; }

        public virtual void Commit()
        {
            CommitAsync().GetAwaiter().GetResult();
        }

        public virtual Task CommitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                Transaction.Commit();
            }
            catch
            {
                Transaction.Rollback();
                throw;
            }
            finally
            {
                Transaction.Dispose();
                Transaction = Connection.BeginTransaction();
            }

            return Task.CompletedTask;
        }

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                DbConnection?.Dispose();
                Transaction?.Dispose();
            }

            _disposed = true;
        }
    }
}
