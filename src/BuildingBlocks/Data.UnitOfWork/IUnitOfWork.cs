using System.Data;

namespace Data.UnitOfWork
{
    public interface IUnitOfWork
    {
        IDbConnection Connection { get; }
        void BeginTransaction(IsolationLevel level = IsolationLevel.ReadCommitted);
        void CommitTransaction();
        void RollbackTransaction();
    }
}
