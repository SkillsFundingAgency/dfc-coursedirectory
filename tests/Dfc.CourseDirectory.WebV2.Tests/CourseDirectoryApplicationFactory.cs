using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.BinaryStorageProvider;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Core.Search.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Cookies;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using FormFlow.State;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
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

                services.AddSingleton<ISearchClient<Onspd>>(OnspdSearchClient.Object);
                services.AddSingleton(BinaryStorageProvider.Object);
            });

        private void ResetMocks()
        {
            OnspdSearchClient.Reset();
            BinaryStorageProvider.Reset();
        }
    }
}
