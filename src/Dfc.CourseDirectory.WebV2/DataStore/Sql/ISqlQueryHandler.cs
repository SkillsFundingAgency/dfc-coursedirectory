using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql
{
    public interface ISqlQueryHandler<TQuery, TResult>
        where TQuery : ISqlQuery<TResult>
    {
        Task<TResult> Execute(SqlTransaction transaction, TQuery query);
    }
}
