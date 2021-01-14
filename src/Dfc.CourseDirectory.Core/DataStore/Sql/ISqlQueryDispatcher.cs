using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Core.DataStore.Sql
{
    public interface ISqlQueryDispatcher
    {
        SqlTransaction Transaction { get; }
        void CreateTransaction(IsolationLevel isolationLevel);
        Task<T> ExecuteQuery<T>(ISqlQuery<T> query);
        IAsyncEnumerable<T> ExecuteQuery<T>(ISqlQuery<IAsyncEnumerable<T>> query);
    }
}
