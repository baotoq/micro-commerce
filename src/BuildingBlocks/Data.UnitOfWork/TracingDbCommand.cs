using System;
using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Logging;

namespace Data.UnitOfWork
{
    [System.ComponentModel.DesignerCategory("Code")]
    public class TracingDbCommand : DbCommand
    {
        private readonly ILogger<TracingDbCommand> _logger;
        private readonly DbCommand _command;

        public TracingDbCommand(ILogger<TracingDbCommand> logger, DbCommand command)
        {
            _logger = logger;
            _command = command;
        }

        public override string CommandText
        {
            get => _command.CommandText;
            set => _command.CommandText = value;
        }

        public override int CommandTimeout
        {
            get => _command.CommandTimeout;
            set => _command.CommandTimeout = value;
        }

        public override CommandType CommandType
        {
            get => _command.CommandType;
            set => _command.CommandType = value;
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get => _command.UpdatedRowSource;
            set => _command.UpdatedRowSource = value;
        }

        protected override DbConnection DbConnection
        {
            get => _command.Connection;
            set => _command.Connection = value;
        }

        protected override DbParameterCollection DbParameterCollection => _command.Parameters;

        protected override DbTransaction DbTransaction
        {
            get => _command.Transaction;
            set => _command.Transaction = value;
        }

        public override bool DesignTimeVisible
        {
            get => _command.DesignTimeVisible;
            set => _command.DesignTimeVisible = value;
        }

        public override void Cancel() => _command.Cancel();

        public override int ExecuteNonQuery() => AddTracing(() => _command.ExecuteNonQuery());

        public override object ExecuteScalar() => AddTracing(() => _command.ExecuteScalar());

        public override void Prepare() => _command.Prepare();

        protected override DbParameter CreateDbParameter() => _command.CreateParameter();

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => AddTracing(() => _command.ExecuteReader(behavior));

        private T AddTracing<T>(Func<T> func)
        {
            _logger.LogInformation(
                "Execute SQL query to database with CommandType: {CommandType}, CommandText: {CommandText}",
                _command.CommandType, _command.CommandText);

            return func();
        }

        private bool _disposed;

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _command?.Dispose();
            }

            _disposed = true;

            base.Dispose(disposing);
        }
    }
}
