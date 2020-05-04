using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb
{
    public interface ICosmosDbQueryDispatcher
    {
        Task<T> ExecuteQuery<T>(ICosmosDbQuery<T> query);
    }
}
