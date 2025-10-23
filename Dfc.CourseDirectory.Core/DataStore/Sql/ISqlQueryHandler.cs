using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Core.DataStore.Sql
{
    public interface ISqlQueryHandler<TQuery, TResult>
        where TQuery : ISqlQuery<TResult>
    {
        Task<TResult> Execute(SqlTransaction transaction, TQuery query);
    }
}
