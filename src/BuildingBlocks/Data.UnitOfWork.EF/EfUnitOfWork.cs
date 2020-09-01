using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Data.UnitOfWork.EF
{
    public class EfUnitOfWork : UnitOfWork
    {
        protected DbContext Context { get; }

        public EfUnitOfWork(DbContext context, IServiceProvider serviceProvider) : base(() => context.Database.GetDbConnection(), serviceProvider)
        {
            Context = context;
        }

        public override void Commit() => Context.SaveChanges();

        public override Task CommitAsync(CancellationToken cancellationToken = default) => Context.SaveChangesAsync(cancellationToken);

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
