using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Dfc.CourseDirectory.WebV2.Services;
using Dfc.CourseDirectory.WebV2.Services.Interfaces;
using Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Moq;
using Respawn;
using Xunit;
using CosmosDbQueryDispatcher = Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.CosmosDbQueryDispatcher;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    [CollectionDefinition("Mvc")]
    public class HttpCollection : ICollectionFixture<CourseDirectoryApplicationFactory>
    {
    }

    public class CourseDirectoryApplicationFactory : WebApplicationFactory<Startup>
    {
        private readonly Checkpoint _sqlCheckpoint;

        public CourseDirectoryApplicationFactory()
        {
            _sqlCheckpoint = CreateCheckpoint();
        }

        public MutableClock Clock => Server.Host.Services.GetRequiredService<IClock>() as MutableClock;

        public IConfiguration Configuration => Server.Host.Services.GetRequiredService<IConfiguration>();

        public Mock<CosmosDbQueryDispatcher> CosmosDbQueryDispatcher =>
            Mock.Get(Services.GetRequiredService<ICosmosDbQueryDispatcher>() as CosmosDbQueryDispatcher);

        public OverridableFeatureFlagProvider FeatureFlagProvider => Services.GetRequiredService<IFeatureFlagProvider>() as OverridableFeatureFlagProvider;

        public HostingOptions HostingOptions => Services.GetRequiredService<HostingOptions>();

        public InMemoryDocumentStore InMemoryDocumentStore => Services.GetRequiredService<InMemoryDocumentStore>();

        public ClearableMemoryCache MemoryCache => Services.GetRequiredService<IMemoryCache>() as ClearableMemoryCache;

        public InMemoryMptxStateProvider MptxStateProvider =>
            Services.GetRequiredService<IMptxStateProvider>() as InMemoryMptxStateProvider;

        public SqlQuerySpy SqlQuerySpy => Services.GetRequiredService<SqlQuerySpy>();

        public TestData TestData => Services.GetRequiredService<TestData>();

        public TestUserInfo User => Services.GetRequiredService<TestUserInfo>();
        public Mock<UkrlpWcfService> UkrlpWcfService => Mock.Get(Services.GetRequiredService<IUkrlpWcfService>() as UkrlpWcfService);

        public void OnTestStarting()
        {
            // Clear calls on any mocks
            ResetMocks();

            // Clear in-memory DB
            InMemoryDocumentStore.Clear();

            // Reset cache
            MemoryCache.Clear();

            // Restore HostingOptions values to default
            HostingOptions.RewriteForbiddenToNotFound = false;

            // Reset the clock
            Clock.UtcNow = MutableClock.Start;

            // Reset feature flag provider
            FeatureFlagProvider.Reset();

            // Reset MPTX state
            MptxStateProvider.Clear();

            // Clear spy calls
            SqlQuerySpy.Reset();

            // Clear StandardsAndFrameworksCache
            Services.GetRequiredService<IStandardsAndFrameworksCache>().Clear();
        }

        public async Task OnTestStarted()
        {
            // Clear out all data from SQL database
            await _sqlCheckpoint.Reset(Configuration["ConnectionStrings:DefaultConnection"]);

            // Reset to the default calling user
            await User.Reset();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseContentRoot(".");

        protected override IWebHostBuilder CreateWebHostBuilder() => WebHost
            .CreateDefaultBuilder()
            .UseEnvironment("Testing")
            .ConfigureAppConfiguration(builder =>
            {
                var fileProvider = new ManifestEmbeddedFileProvider(typeof(CourseDirectoryApplicationFactory).Assembly);

                builder
                    .AddJsonFile(fileProvider, "appsettings.Testing.json", optional: false, reloadOnChange: false)
                    .AddEnvironmentVariables();
            })
            .UseStartup<Startup>()
            .ConfigureServices(services =>
            {
                services.AddSingleton<IFeatureFlagProvider, ConfigurationFeatureFlagProvider>();
                services.Decorate<IFeatureFlagProvider, OverridableFeatureFlagProvider>();
            });

        private void ResetMocks()
        {
        }

        private Checkpoint CreateCheckpoint() => new Checkpoint()
        {
            SchemasToInclude = new[] { "Pttcd" }
        };
    }
}
