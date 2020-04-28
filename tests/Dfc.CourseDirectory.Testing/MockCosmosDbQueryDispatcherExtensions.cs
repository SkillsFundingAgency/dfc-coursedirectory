using System;
using System.Linq.Expressions;
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

        public static void VerifyExecuteQuery<TQuery, TResult>(
            this Mock<CosmosDbQueryDispatcher> mock,
            Expression<Func<TQuery, bool>> match)
            where TQuery : ICosmosDbQuery<TResult>
        {
            mock.Verify(mock => mock.ExecuteQuery(It.Is(match)));
        }
    }
}