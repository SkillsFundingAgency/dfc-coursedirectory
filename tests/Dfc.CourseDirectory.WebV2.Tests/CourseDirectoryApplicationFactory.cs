using System.Threading.Tasks;
using Dfc.CourseDirectory.Testing;
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

        public SingletonSession Session => ((SingletonSessionStore)Services.GetRequiredService<ISessionStore>()).Instance;

        public Settings Settings => Services.GetRequiredService<Settings>();

        public SqlQuerySpy SqlQuerySpy => DatabaseFixture.SqlQuerySpy;

        public TestData TestData => DatabaseFixture.TestData;

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

            // Restore HostingOptions values to default
            Settings.RewriteForbiddenToNotFound = false;

            Clock.UtcNow = MutableClock.Start;

            FeatureFlagProvider.Reset();

            MptxStateProvider.Clear();

            Services.GetRequiredService<IStandardsAndFrameworksCache>().Clear();

            CookieSettingsProvider.Reset();

            (Services.GetRequiredService<IUserInstanceStateStore>() as TestUserInstanceStateStore)?.Clear();

            Session.Clear();
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
