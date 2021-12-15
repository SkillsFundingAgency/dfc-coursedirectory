using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Dfc.CourseDirectory.Core.BinaryStorageProvider;
using Dfc.CourseDirectory.Core.Configuration;
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Core.Search.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.AddressSearch;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using FormFlow;
using FormFlow.State;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

        protected Mock<IAddressSearchService> AddressSearchService => Factory.AddressSearchService;

        protected Mock<BlobServiceClient> BlobServiceClient => Factory.BlobServiceClient;

        protected MutableClock Clock => Factory.Clock;

        protected TestCookieSettingsProvider CookieSettingsProvider => Factory.CookieSettingsProvider;

        protected Mock<CosmosDbQueryDispatcher> CosmosDbQueryDispatcher => Factory.CosmosDbQueryDispatcher;

        protected T CreateInstance<T>(params object[] parameters) =>
            ActivatorUtilities.CreateInstance<T>(Factory.Services, parameters);

        protected CourseDirectoryApplicationFactory Factory { get; }

        protected HttpClient HttpClient { get; set; }

        protected MptxManager MptxManager => Factory.MptxManager;

        protected IRegionCache RegionCache => Factory.RegionCache;

        protected Mock<ISearchClient<Provider>> ProviderSearchClient => Factory.ProviderSearchClient;
        public Mock<ISearchClient<Lars>> LarsSearchClient => Factory.LarsSearchClient;
        public Mock<IOptions<LarsSearchSettings>> LarsSearchSettings => Factory.LarsSearchSettings;

        protected Mock<IBinaryStorageProvider> BinaryStorageProvider => Factory.BinaryStorageProvider;

        protected SingletonSession Session => Factory.Session;

        protected SqlQuerySpy SqlQuerySpy => Factory.SqlQuerySpy;

        protected TestData TestData => Factory.TestData;

        protected TestUserInfo User => Factory.User;

        public Task DisposeAsync() => Task.CompletedTask;

        public Task InitializeAsync() => Factory.OnTestStartingAsync();

        protected JourneyInstance<TState> CreateJourneyInstance<TState>(
            string journeyName,
            Action<KeysBuilder> configureKeys,
            TState state,
            IReadOnlyDictionary<object, object> properties = null,
            string uniqueKey = null)
        {
            var keysBuilder = new KeysBuilder();
            configureKeys(keysBuilder);

            if (uniqueKey != null)
            {
                keysBuilder.With("ffiid", uniqueKey);
            }

            var keys = keysBuilder.Build();

            var instanceId = new JourneyInstanceId(journeyName, keys);

            var instanceStateProvider = Factory.Services.GetRequiredService<IUserInstanceStateProvider>();

            return (JourneyInstance<TState>)instanceStateProvider.CreateInstance(
                journeyName,
                instanceId,
                typeof(TState),
                state,
                properties);
        }

        protected MptxInstanceContext<TState> CreateMptxInstance<TState>(TState state)
            where TState : IMptxState =>
            MptxManager.CreateInstance(state);

        protected JourneyInstance<TState> GetJourneyInstance<TState>(
            string journeyName,
            Action<KeysBuilder> configureKeys,
            string uniqueKey = null)
        {
            var keysBuilder = new KeysBuilder();
            configureKeys(keysBuilder);

            if (uniqueKey != null)
            {
                keysBuilder.With("ffiid", uniqueKey);
            }

            var keys = keysBuilder.Build();

            var instanceId = new JourneyInstanceId(journeyName, keys);

            var instanceStateProvider = Factory.Services.GetRequiredService<IUserInstanceStateProvider>();

            return (JourneyInstance<TState>)instanceStateProvider.GetInstance(instanceId);
        }

        protected JourneyInstance<TState> GetJourneyInstance<TState>(JourneyInstanceId instanceId)
        {
            var instanceStateProvider = Factory.Services.GetRequiredService<IUserInstanceStateProvider>();

            return (JourneyInstance<TState>)instanceStateProvider.GetInstance(instanceId);
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
