using System;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Moq;
using CosmosDbQueryDispatcher = Dfc.CourseDirectory.Testing.DataStore.CosmosDb.CosmosDbQueryDispatcher;

namespace Dfc.CourseDirectory.Testing
{
    public static class MockCosmosDbQueryDispatcherExtensions
    {
        public static void Callback<TQuery, TResult>(
            this Mock<CosmosDbQueryDispatcher> mock,
            Action<TQuery> action)
            where TQuery : ICosmosDbQuery<TResult>
        {
            mock
                .Setup(mock => mock.ExecuteQuery(It.IsAny<TQuery>()))
                .CallBase()
                .Callback<ICosmosDbQuery<TResult>>(q => action((TQuery)q));
        }
    }
}
