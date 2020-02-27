using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql
{
    public interface ISqlQueryDispatcher
    {
        Task<T> ExecuteQuery<T>(ISqlQuery<T> query);
    }
}
