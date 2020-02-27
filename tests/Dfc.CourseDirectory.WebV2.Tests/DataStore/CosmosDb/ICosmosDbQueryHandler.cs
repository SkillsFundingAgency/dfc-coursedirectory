using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;

namespace Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb
{
    public interface ICosmosDbQueryHandler<TRequest, TResult>
        where TRequest : ICosmosDbQuery<TResult>
    {
        TResult Execute(InMemoryDocumentStore inMemoryDocumentStore, TRequest request);
    }
}
