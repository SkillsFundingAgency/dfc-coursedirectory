using System.Data.SqlClient;

namespace Dfc.CourseDirectory.Core.DataStore.Sql
{
    public class SqlTransactionMarker
    {
        public bool GotTransaction => Transaction != null;
        public SqlTransaction Transaction { get; private set; }

        public void OnTransactionCompleted() => Transaction = null;
        public void OnTransactionCreated(SqlTransaction transaction) => Transaction = transaction;
    }
}
