using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Core.DataStore.Sql
{
    public interface ISqlQueryDispatcher
    {
        SqlTransaction Transaction { get; }
        Task<T> ExecuteQuery<T>(ISqlQuery<T> query);
    }
}
