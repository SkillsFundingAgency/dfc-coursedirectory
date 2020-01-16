using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb
{
    public interface ICosmosDbQueryDispatcher
    {
        Task<T> ExecuteQuery<T>(ICosmosDbQuery<T> query);
    }
}
