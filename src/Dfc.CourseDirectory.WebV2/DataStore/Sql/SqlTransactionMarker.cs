using System.Data.SqlClient;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql
{
    public class SqlTransactionMarker
    {
        public bool GotTransaction => Transaction != null;
        public SqlTransaction Transaction { get; private set; }

        public void OnTransactionCreated(SqlTransaction transaction) => Transaction = transaction;
    }
}
