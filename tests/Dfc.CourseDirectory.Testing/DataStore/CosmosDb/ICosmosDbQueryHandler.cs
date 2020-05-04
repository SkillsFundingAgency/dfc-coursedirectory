using Dfc.CourseDirectory.Core.DataStore.CosmosDb;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb
{
    public interface ICosmosDbQueryHandler<TRequest, TResult>
        where TRequest : ICosmosDbQuery<TResult>
    {
        TResult Execute(InMemoryDocumentStore inMemoryDocumentStore, TRequest request);
    }
}
