using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.BinaryStorageProvider;
using Dfc.CourseDirectory.Core.Configuration;
using Dfc.CourseDirectory.Core.DataStore;
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
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Xunit.Abstractions;

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

        public Mock<BlobServiceClient> BlobServiceClient { get; } = new Mock<BlobServiceClient>();

        public MutableClock Clock => DatabaseFixture.Clock;

        public IConfiguration Configuration => Server.Host.Services.GetRequiredService<IConfiguration>();

        public TestCookieSettingsProvider CookieSettingsProvider => Services.GetRequiredService<ICookieSettingsProvider>() as TestCookieSettingsProvider;


        public DatabaseFixture DatabaseFixture { get; }

        public OverridableFeatureFlagProvider FeatureFlagProvider => Services.GetRequiredService<IFeatureFlagProvider>() as OverridableFeatureFlagProvider;

        public ClearableMemoryCache MemoryCache => Services.GetRequiredService<IDistributedCache>() as ClearableMemoryCache;

        public MptxManager MptxManager => Services.GetRequiredService<MptxManager>();

        public InMemoryMptxStateProvider MptxStateProvider =>
            Services.GetRequiredService<IMptxStateProvider>() as InMemoryMptxStateProvider;

        public IRegionCache RegionCache => Services.GetRequiredService<IRegionCache>();

        public Mock<ISearchClient<Provider>> ProviderSearchClient { get; } = new Mock<ISearchClient<Provider>>();
        public Mock<ISearchClient<Lars>> LarsSearchClient { get; } = new Mock<ISearchClient<Lars>>();
        public Mock<IOptions<LarsSearchSettings>> LarsSearchSettings { get; } = new Mock<IOptions<LarsSearchSettings>>();

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

                services.AddSingleton(ProviderSearchClient.Object);
                services.AddSingleton(BinaryStorageProvider.Object);
                services.AddSingleton(AddressSearchService.Object);
                services.AddSingleton(BlobServiceClient.Object);
                services.AddSingleton(LarsSearchClient.Object);
                services.AddSingleton(LarsSearchSettings.Object);
            });

        private void ResetMocks()
        {
            AddressSearchService.Reset();
            BlobServiceClient.Reset();
            ProviderSearchClient.Reset();
            BinaryStorageProvider.Reset();
            LarsSearchClient.Reset();
            LarsSearchSettings.Reset();
        }
    }
}
