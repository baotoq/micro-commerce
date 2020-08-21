using System;
using System.Data;

namespace Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        protected readonly Func<IDbConnection> ConnFactoryFunc;
        protected IDbConnection DbConnection;
        protected IDbTransaction Transaction;

        public UnitOfWork(Func<IDbConnection> connFactoryFunc)
        {
            ConnFactoryFunc = connFactoryFunc;
        }

        public IDbConnection Connection => DbConnection ??= ConnFactoryFunc();

        public void BeginTransaction(IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            Transaction = Connection.BeginTransaction(level);
        }

        public void CommitTransaction()
        {
            Transaction.Commit();
        }

        public void RollbackTransaction()
        {
            Transaction.Rollback();
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
