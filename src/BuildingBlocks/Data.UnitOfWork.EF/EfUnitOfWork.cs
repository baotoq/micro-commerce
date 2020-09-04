using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Data.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Data.UnitOfWork.EF
{
    public class EfUnitOfWork : IUnitOfWork, IDisposable
    {
        protected DbContext Context { get; }
        protected IServiceProvider ServiceProvider { get; }

        public EfUnitOfWork(DbContext context, IServiceProvider serviceProvider)
        {
            Context = context;
            ServiceProvider = serviceProvider;
        }

        public IRepository<TEntity> Repository<TEntity>() where TEntity : IEntity<long> => ServiceProvider.GetRequiredService<IRepository<TEntity>>();

        public IRepository<TEntity, TId> Repository<TEntity, TId>() where TEntity : IEntity<TId> => ServiceProvider.GetRequiredService<IRepository<TEntity, TId>>();

        public DbConnection Connection => Context.Database.GetDbConnection();
        public DbTransaction Transaction => throw new NotSupportedException();

        public void Commit() => Context.SaveChanges();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Context.SaveChangesAsync(cancellationToken);

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

        ~EfUnitOfWork()
        {
            Dispose(false);
        }
    }
}
