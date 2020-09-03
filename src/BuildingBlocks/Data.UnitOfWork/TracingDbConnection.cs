using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Logging;

namespace Data.UnitOfWork
{
    [System.ComponentModel.DesignerCategory("Code")]
    public class TracingDbConnection : DbConnection
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly DbConnection _connection;

        public TracingDbConnection(DbConnection connection, ILoggerFactory loggerFactory)
        {
            _connection = connection;
            _loggerFactory = loggerFactory;
        }

        public override string ConnectionString
        {
            get => _connection.ConnectionString;
            set => _connection.ConnectionString = value;
        }

        public override string Database => _connection.Database;

        public override string DataSource => _connection.DataSource;

        public override string ServerVersion => _connection.ServerVersion;

        public override ConnectionState State => _connection.State;

        public override void ChangeDatabase(string databaseName) => _connection.ChangeDatabase(databaseName);

        public override void Close() => _connection.Close();

        public override void Open() => _connection.Open();

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => _connection.BeginTransaction(isolationLevel);

        protected override DbCommand CreateDbCommand() => new TracingDbCommand(_loggerFactory.CreateLogger<TracingDbCommand>(), _connection.CreateCommand());

        private bool _disposed;

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _connection?.Dispose();
            }

            _disposed = true;

            base.Dispose(disposing);
        }
    }
}
