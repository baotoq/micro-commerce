using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Data.Entities.Models;
using Data.UnitOfWork.EF.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Data.UnitOfWork.EF
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        protected DbContext Context { get; }
        protected IServiceProvider ServiceProvider { get; }
        
        public UnitOfWork(DbContext context, IServiceProvider serviceProvider)
        {
            Context = context;
            ServiceProvider = serviceProvider;
        }

        public IRepository<TEntity> Repository<TEntity>() where TEntity : IEntity<long> => ServiceProvider.GetRequiredService<IRepository<TEntity>>();

        public IRepository<TEntity, TId> Repository<TEntity, TId>() where TEntity : IEntity<TId> => ServiceProvider.GetRequiredService<IRepository<TEntity, TId>>();

        public DbConnection Connection => Context.Database.GetDbConnection();

        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) => Context.Database.BeginTransactionAsync(cancellationToken);
        public Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
            => Context.Database.BeginTransactionAsync(isolationLevel, cancellationToken);

        public void SaveChanges() => Context.SaveChanges();

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Context.SaveChangesAsync(cancellationToken);

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
                Context?.Dispose();
            }

            _disposed = true;
        }

        ~UnitOfWork()
        {
            Dispose(false);
        }
    }
}
