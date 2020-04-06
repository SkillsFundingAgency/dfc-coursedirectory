using System;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Moq;
using CosmosDbQueryDispatcher = Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.CosmosDbQueryDispatcher;

namespace Dfc.CourseDirectory.WebV2.Tests
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
