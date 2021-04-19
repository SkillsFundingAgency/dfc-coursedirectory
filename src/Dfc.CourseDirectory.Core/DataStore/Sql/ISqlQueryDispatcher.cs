using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Core.DataStore.Sql
{
    public interface ISqlQueryDispatcher : IDisposable
    {
        SqlTransaction Transaction { get; }
        Task Commit();
        Task<T> ExecuteQuery<T>(ISqlQuery<T> query);
        IAsyncEnumerable<T> ExecuteQuery<T>(ISqlQuery<IAsyncEnumerable<T>> query);
    }
}
