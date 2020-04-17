using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using CosmosDbQueryDispatcher = Dfc.CourseDirectory.Testing.DataStore.CosmosDb.CosmosDbQueryDispatcher;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    [Collection("Mvc")]
    [Trait("SkipOnCI", "true")]  // Until we have SQL DB on CI
    public abstract class MvcTestBase : IAsyncLifetime
    {
        public MvcTestBase(CourseDirectoryApplicationFactory factory)
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

        protected T CreateInstance<T>(params object[] parameters) =>
            ActivatorUtilities.CreateInstance<T>(Factory.Services, parameters);

        protected CourseDirectoryApplicationFactory Factory { get; }

        protected HttpClient HttpClient { get; }

        protected MptxManager MptxManager => Factory.MptxManager;

        protected SqlQuerySpy SqlQuerySpy => Factory.SqlQuerySpy;

        protected TestData TestData => Factory.TestData;

        protected TestUserInfo User => Factory.User;

        public Task DisposeAsync() => Task.CompletedTask;

        public Task InitializeAsync() => Factory.OnTestStartingAsync();

        protected MptxInstanceContext<TState> CreateMptxInstance<TState>(TState state)
            where TState : IMptxState =>
            MptxManager.CreateInstance(state);

        protected MptxInstanceContext<TState> GetMptxInstance<TState>(string instanceId)
            where TState : IMptxState =>
            (MptxInstanceContext<TState>)MptxManager.GetInstance(instanceId);

        protected Task WithSqlQueryDispatcher(Func<ISqlQueryDispatcher, Task> action) =>
            Factory.DatabaseFixture.WithSqlQueryDispatcher(action);

        protected Task<TResult> WithSqlQueryDispatcher<TResult>(
            Func<ISqlQueryDispatcher, Task<TResult>> action) =>
            Factory.DatabaseFixture.WithSqlQueryDispatcher(action);
    }
}
