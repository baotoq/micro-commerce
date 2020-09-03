using System;
using System.Data.Common;

namespace Data.UnitOfWork
{
    public interface IConnectionFactory
    {
        DbConnection CreateDbConnection();
    }

    public class ConnectionFactory : IConnectionFactory
    {
        private readonly Func<DbConnection> _connectionFunc;

        public ConnectionFactory(Func<DbConnection> connectionFunc)
        {
            _connectionFunc = connectionFunc;
        }

        public DbConnection CreateDbConnection() => _connectionFunc();
    }
}
