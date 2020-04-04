using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using CosmosDbQueryDispatcher = Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.CosmosDbQueryDispatcher;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    [Collection("Mvc")]
    [Trait("SkipOnCI", "true")]  // Until we have SQL DB on CI
    public abstract class TestBase : IAsyncLifetime
    {
        public TestBase(CourseDirectoryApplicationFactory factory)
        {
            Factory = factory;

            HttpClient = factory.CreateClient(new WebApplicationFactoryClientOptions()
            {
                AllowAutoRedirect = false
            });
            Factory.OnTestStarting();
        }

        protected MutableClock Clock => Factory.Clock;

        protected Mock<CosmosDbQueryDispatcher> CosmosDbQueryDispatcher => Factory.CosmosDbQueryDispatcher;

        protected CourseDirectoryApplicationFactory Factory { get; }

        protected HttpClient HttpClient { get; }

        protected MptxManager MptxManager => Factory.MptxManager;

        protected SqlQuerySpy SqlQuerySpy => Factory.SqlQuerySpy;

        protected TestData TestData => Factory.TestData;

        protected TestUserInfo User => Factory.User;

        public Task DisposeAsync() => Task.CompletedTask;

        public Task InitializeAsync() => Factory.OnTestStarted();

        protected MptxInstanceContext<TState> CreateMptxInstance<TState>(string flowName, TState state)
            where TState : IMptxState =>
            MptxManager.CreateInstance(flowName, state);

        protected MptxInstanceContext<TState> GetMptxInstance<TState>(string instanceId)
            where TState : IMptxState =>
            (MptxInstanceContext<TState>)MptxManager.GetInstance(instanceId);

        protected Task WithSqlQueryDispatcher(Func<ISqlQueryDispatcher, Task> action) =>
            WithSqlQueryDispatcher(async dispatcher =>
            {
                await action(dispatcher);
                return 0;
            });

        protected async Task<TResult> WithSqlQueryDispatcher<TResult>(
            Func<ISqlQueryDispatcher, Task<TResult>> action)
        {
            var serviceScopeFactory = Factory.Services.GetRequiredService<IServiceScopeFactory>();
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var transaction = scope.ServiceProvider.GetRequiredService<SqlTransaction>();
                var queryDispatcher = scope.ServiceProvider.GetRequiredService<ISqlQueryDispatcher>();

                var result = await action(queryDispatcher);

                transaction.Commit();

                return result;
            }
        }
    }
}
