using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.BinaryStorageProvider;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Core.Search.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.AddressSearch;
using Dfc.CourseDirectory.WebV2.Cookies;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using FormFlow.State;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Xunit.Abstractions;
using CosmosDbQueryDispatcher = Dfc.CourseDirectory.Testing.DataStore.CosmosDb.CosmosDbQueryDispatcher;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    [CollectionDefinition("Mvc")]
    public class MvcCollection : ICollectionFixture<CourseDirectoryApplicationFactory>
    {
    }

    public class CourseDirectoryApplicationFactory : WebApplicationFactory<Startup>
    {
        public CourseDirectoryApplicationFactory(IMessageSink messageSink)
        {
            DatabaseFixture = new DatabaseFixture(Configuration, Server.Host.Services, messageSink);
            TestData = DatabaseFixture.CreateTestData();
        }

        public Mock<IAddressSearchService> AddressSearchService { get; } = new Mock<IAddressSearchService>();

        public MutableClock Clock => DatabaseFixture.Clock;

        public IConfiguration Configuration => Server.Host.Services.GetRequiredService<IConfiguration>();

        public TestCookieSettingsProvider CookieSettingsProvider => Services.GetRequiredService<ICookieSettingsProvider>() as TestCookieSettingsProvider;

        public Mock<CosmosDbQueryDispatcher> CosmosDbQueryDispatcher => DatabaseFixture.CosmosDbQueryDispatcher;

        public DatabaseFixture DatabaseFixture { get; }

        public OverridableFeatureFlagProvider FeatureFlagProvider => Services.GetRequiredService<IFeatureFlagProvider>() as OverridableFeatureFlagProvider;

        public ClearableMemoryCache MemoryCache => Services.GetRequiredService<IDistributedCache>() as ClearableMemoryCache;

        public MptxManager MptxManager => Services.GetRequiredService<MptxManager>();

        public InMemoryMptxStateProvider MptxStateProvider =>
            Services.GetRequiredService<IMptxStateProvider>() as InMemoryMptxStateProvider;

        public Mock<ISearchClient<Onspd>> OnspdSearchClient { get; } = new Mock<ISearchClient<Onspd>>();

        public Mock<ISearchClient<Provider>> ProviderSearchClient { get; } = new Mock<ISearchClient<Provider>>();

        public Mock<IBinaryStorageProvider> BinaryStorageProvider { get; } = new Mock<IBinaryStorageProvider>();

        public SingletonSession Session => ((SingletonSessionStore)Services.GetRequiredService<ISessionStore>()).Instance;

        public SqlQuerySpy SqlQuerySpy => DatabaseFixture.SqlQuerySpy;

        public TestData TestData { get; }

        public TestUserInfo User => Services.GetRequiredService<TestUserInfo>();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DatabaseFixture.Dispose();
            }
        }

        public void OnTestStarting()
        {
            DatabaseFixture.OnTestStarting();

            ResetMocks();

            MemoryCache.Clear();

            Clock.UtcNow = MutableClock.Start;

            FeatureFlagProvider.Reset();

            MptxStateProvider.Clear();

            Services.GetRequiredService<IStandardsAndFrameworksCache>().Clear();

            CookieSettingsProvider.Reset();

            (Services.GetRequiredService<IUserInstanceStateStore>() as TestUserInstanceStateStore)?.Clear();

            Session.Clear();

            BinaryStorageProvider.SetReturnsDefault(Task.FromResult<IReadOnlyCollection<BlobFileInfo>>(Array.Empty<BlobFileInfo>()));
        }

        public async Task OnTestStartingAsync()
        {
            await DatabaseFixture.OnTestStartingAsync();

            // Reset to the default calling user
            await User.Reset();
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseContentRoot(".");

        protected override IWebHostBuilder CreateWebHostBuilder() => WebHost
            .CreateDefaultBuilder()
            .UseEnvironment("Testing")
            .ConfigureAppConfiguration(builder => builder.AddTestConfigurationSources())
            .UseStartup<Startup>()
            .ConfigureServices(services =>
            {
                ConfigureServices(services);

                services.AddSingleton(OnspdSearchClient.Object);
                services.AddSingleton(ProviderSearchClient.Object);
                services.AddSingleton(BinaryStorageProvider.Object);
                services.AddSingleton(AddressSearchService.Object);
            });

        private void ResetMocks()
        {
            AddressSearchService.Reset();
            OnspdSearchClient.Reset();
            ProviderSearchClient.Reset();
            BinaryStorageProvider.Reset();
        }
    }
}
