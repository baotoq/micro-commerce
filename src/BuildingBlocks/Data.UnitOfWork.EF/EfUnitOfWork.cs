using System;
using System.Threading;
using System.Threading.Tasks;
using Data.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Data.UnitOfWork.EF
{
    public class EfUnitOfWork : UnitOfWork, IEfUnitOfWork
    {
        protected DbContext Context { get; }
        protected IServiceProvider ServiceProvider { get; }

        public EfUnitOfWork(DbContext context, IServiceProvider serviceProvider) : base(() => context.Database.GetDbConnection())
        {
            Context = context;
            ServiceProvider = serviceProvider;
        }

        public IRepository<TEntity> Repository<TEntity>() where TEntity : IEntity<long> => ServiceProvider.GetRequiredService<IRepository<TEntity>>();

        public IRepository<TEntity, TId> Repository<TEntity, TId>() where TEntity : IEntity<TId> => ServiceProvider.GetRequiredService<IRepository<TEntity, TId>>();

        public int SaveChanges() => Context.SaveChanges();

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Context.SaveChangesAsync(cancellationToken);

        private bool _disposed;

        protected override void Dispose(bool disposing)
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

            base.Dispose(disposing);
        }
    }
}
