using System.Collections.Generic;
using System.Data.SqlClient;

namespace Dfc.CourseDirectory.Core.DataStore.Sql
{
    public interface ISqlAsyncEnumerableQueryHandler<TQuery, TResult>
            where TQuery : ISqlQuery<IAsyncEnumerable<TResult>>
    {
        IAsyncEnumerable<TResult> Execute(SqlTransaction transaction, TQuery query);
    }
}
