using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Core.Search.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using FormFlow;
using FormFlow.State;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using CosmosDbQueryDispatcher = Dfc.CourseDirectory.Testing.DataStore.CosmosDb.CosmosDbQueryDispatcher;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    [Collection("Mvc")]
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

        protected TestCookieSettingsProvider CookieSettingsProvider => Factory.CookieSettingsProvider;

        protected Mock<CosmosDbQueryDispatcher> CosmosDbQueryDispatcher => Factory.CosmosDbQueryDispatcher;

        protected T CreateInstance<T>(params object[] parameters) =>
            ActivatorUtilities.CreateInstance<T>(Factory.Services, parameters);

        protected CourseDirectoryApplicationFactory Factory { get; }

        protected HttpClient HttpClient { get; set; }

        protected MptxManager MptxManager => Factory.MptxManager;

        protected Mock<ISearchClient<Onspd>> OnspdSearchClient => Factory.OnspdSearchClient;

        protected SingletonSession Session => Factory.Session;

        protected SqlQuerySpy SqlQuerySpy => Factory.SqlQuerySpy;

        protected TestData TestData => Factory.TestData;

        protected TestUserInfo User => Factory.User;

        public Task DisposeAsync() => Task.CompletedTask;

        public Task InitializeAsync() => Factory.OnTestStartingAsync();

        protected FormFlowInstance<TState> CreateFormFlowInstanceForRouteParameters<TState>(
            string key,
            IReadOnlyDictionary<string, object> routeParameters,
            TState state,
            IReadOnlyDictionary<object, object> properties = null)
        {
            var instanceId = FormFlowInstanceId.GenerateForRouteValues(key, routeParameters);

            var instanceStateProvider = Factory.Services.GetRequiredService<IUserInstanceStateProvider>();

            return (FormFlowInstance<TState>)instanceStateProvider.CreateInstance(
                key,
                instanceId,
                typeof(TState),
                state,
                properties);
        }

        protected MptxInstanceContext<TState> CreateMptxInstance<TState>(TState state)
            where TState : IMptxState =>
            MptxManager.CreateInstance(state);

        protected FormFlowInstance<TState> GetFormFlowInstanceForRouteParameters<TState>(
            string key,
            IReadOnlyDictionary<string, object> routeParameters)
        {
            var instanceId = FormFlowInstanceId.GenerateForRouteValues(key, routeParameters);

            var instanceStateProvider = Factory.Services.GetRequiredService<IUserInstanceStateProvider>();

            return (FormFlowInstance<TState>)instanceStateProvider.GetInstance(instanceId);
        }

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
