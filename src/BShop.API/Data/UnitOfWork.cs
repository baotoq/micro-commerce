using System;
using System.Threading;
using System.Threading.Tasks;
using BShop.API.Data.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BShop.API.Data
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

        public IRepository<TEntity> Repository<TEntity>() where TEntity : Entity
            => ServiceProvider.GetRequiredService<IRepository<TEntity>>();

        public int SaveChanges() => Context.SaveChanges();

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => Context.SaveChangesAsync(cancellationToken);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Context?.Dispose();
        }
    }
}
