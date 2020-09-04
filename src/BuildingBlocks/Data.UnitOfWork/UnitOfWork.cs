using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Data.Entities.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ILogger<UnitOfWork> _logger;
        protected readonly IConnectionFactory ConnectionFactory;
        protected readonly IServiceProvider ServiceProvider;
        protected DbConnection DbConnection;

        public UnitOfWork(ILogger<UnitOfWork> logger, IConnectionFactory connectionFactory, IServiceProvider serviceProvider)
        {
            _logger = logger;
            ConnectionFactory = connectionFactory;
            ServiceProvider = serviceProvider;
            Transaction = BeginTransaction();
        }

        public virtual IRepository<TEntity> Repository<TEntity>() where TEntity : IEntity<long> => ServiceProvider.GetRequiredService<IRepository<TEntity>>();

        public virtual IRepository<TEntity, TId> Repository<TEntity, TId>() where TEntity : IEntity<TId> => ServiceProvider.GetRequiredService<IRepository<TEntity, TId>>();

        public virtual DbConnection Connection => DbConnection ??= ConnectionFactory.CreateDbConnection();
        public virtual DbTransaction Transaction { get; private set; }

        public DbTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) => Connection.BeginTransaction(isolationLevel);
        public ValueTask<DbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
            => Connection.BeginTransactionAsync(isolationLevel, cancellationToken);

        public void Rollback() => Transaction.Rollback();
        public void RollbackAsync(CancellationToken cancellationToken = default) => Transaction.RollbackAsync(cancellationToken);

        public virtual void Commit()
        {
            try
            {
                Transaction.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There is exception when call commit transaction");
                Rollback();
                throw;
            }
            finally
            {
                Transaction.Dispose();
                Connection.Dispose();
                Transaction = BeginTransaction();
            }
        }

        public virtual async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await Transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There is exception when call commit transaction");
                RollbackAsync(cancellationToken);
                throw;
            }
            finally
            {
                await Transaction.DisposeAsync();
                await Connection.DisposeAsync();
                Transaction = await BeginTransactionAsync(cancellationToken: cancellationToken);
            }
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

        ~UnitOfWork()
        {
            Dispose(false);
        }
    }
}
