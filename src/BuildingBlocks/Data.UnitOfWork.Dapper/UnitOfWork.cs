using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Data.Entities.Models;
using Data.UnitOfWork.Dapper.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Data.UnitOfWork.Dapper
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
        }

        public IRepository<TEntity> Repository<TEntity>() where TEntity : IEntity<long> => ServiceProvider.GetRequiredService<IRepository<TEntity>>();

        public IRepository<TEntity, TId> Repository<TEntity, TId>() where TEntity : IEntity<TId> => ServiceProvider.GetRequiredService<IRepository<TEntity, TId>>();

        public DbConnection Connection => DbConnection ??= ConnectionFactory.CreateDbConnection();

        public DbTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) => Connection.BeginTransaction(isolationLevel);
        public ValueTask<DbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
            => Connection.BeginTransactionAsync(isolationLevel, cancellationToken);

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
            }

            _disposed = true;
        }

        ~UnitOfWork()
        {
            Dispose(false);
        }
    }
}
