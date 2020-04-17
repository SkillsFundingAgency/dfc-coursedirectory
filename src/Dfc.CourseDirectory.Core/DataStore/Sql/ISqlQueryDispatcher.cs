using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Core.DataStore.Sql
{
    public interface ISqlQueryDispatcher
    {
        Task<T> ExecuteQuery<T>(ISqlQuery<T> query);
    }
}
