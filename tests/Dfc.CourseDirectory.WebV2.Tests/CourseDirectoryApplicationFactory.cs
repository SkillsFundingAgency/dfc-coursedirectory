using System.Threading.Tasks;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
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
        }

        public MutableClock Clock => DatabaseFixture.Clock;

        public IConfiguration Configuration => Server.Host.Services.GetRequiredService<IConfiguration>();

        public Mock<CosmosDbQueryDispatcher> CosmosDbQueryDispatcher => DatabaseFixture.CosmosDbQueryDispatcher;

        public DatabaseFixture DatabaseFixture { get; }

        public OverridableFeatureFlagProvider FeatureFlagProvider => Services.GetRequiredService<IFeatureFlagProvider>() as OverridableFeatureFlagProvider;

        public ClearableMemoryCache MemoryCache => Services.GetRequiredService<IDistributedCache>() as ClearableMemoryCache;

        public MptxManager MptxManager => Services.GetRequiredService<MptxManager>();

        public InMemoryMptxStateProvider MptxStateProvider =>
            Services.GetRequiredService<IMptxStateProvider>() as InMemoryMptxStateProvider;

        public Settings Settings => Services.GetRequiredService<Settings>();

        public SqlQuerySpy SqlQuerySpy => DatabaseFixture.SqlQuerySpy;

        public TestData TestData => DatabaseFixture.TestData;

        public TestUserInfo User => Services.GetRequiredService<TestUserInfo>();

        public void OnTestStarting()
        {
            DatabaseFixture.OnTestStarting();

            // Clear calls on any mocks
            ResetMocks();

            // Reset cache
            MemoryCache.Clear();

            // Restore HostingOptions values to default
            Settings.RewriteForbiddenToNotFound = false;

            // Reset the clock
            Clock.UtcNow = MutableClock.Start;

            // Reset feature flag provider
            FeatureFlagProvider.Reset();

            // Reset MPTX state
            MptxStateProvider.Clear();

            // Clear StandardsAndFrameworksCache
            Services.GetRequiredService<IStandardsAndFrameworksCache>().Clear();
        }

        public async Task OnTestStartingAsync()
        {
            await DatabaseFixture.OnTestStartingAsync();

            // Reset to the default calling user
            await User.Reset();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseContentRoot(".");

        protected override IWebHostBuilder CreateWebHostBuilder() => WebHost
            .CreateDefaultBuilder()
            .UseEnvironment("Testing")
            .ConfigureAppConfiguration(builder => builder.AddTestConfigurationSources())
            .UseStartup<Startup>();

        private void ResetMocks()
        {
        }
    }
}
