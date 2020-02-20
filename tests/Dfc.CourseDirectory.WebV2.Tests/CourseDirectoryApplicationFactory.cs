using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Respawn;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class CourseDirectoryApplicationFactory : WebApplicationFactory<Startup>
    {
        private readonly Checkpoint _sqlCheckpoint;

        public CourseDirectoryApplicationFactory()
        {
            _sqlCheckpoint = CreateCheckpoint();
        }

        public IConfiguration Configuration => Server.Host.Services.GetRequiredService<IConfiguration>();

        public HostingOptions HostingOptions => Services.GetRequiredService<HostingOptions>();

        public InMemoryDocumentStore InMemoryDocumentStore => Services.GetRequiredService<InMemoryDocumentStore>();

        public ClearableMemoryCache MemoryCache => Services.GetRequiredService<IMemoryCache>() as ClearableMemoryCache;

        public IServiceProvider Services => Server.Host.Services;

        public TestData TestData => Services.GetRequiredService<TestData>();

        public AuthenticatedUserInfo User => Services.GetRequiredService<AuthenticatedUserInfo>();

        public void OnTestStarting()
        {
            // Clear calls on any mocks
            ResetMocks();

            // Reset to the default calling user
            User.Reset();

            // Clear in-memory DB
            InMemoryDocumentStore.Clear();

            // Reset cache
            MemoryCache.Clear();

            // Restore HostingOptions values to default
            HostingOptions.RewriteForbiddenToNotFound = true;
        }

        public async Task OnTestStarted()
        {
            // Clear out all data from SQL database
            await _sqlCheckpoint.Reset(Configuration["ConnectionStrings:DefaultConnection"]);
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
            .UseStartup<Startup>();

        private void ResetMocks()
        {
        }

        private Checkpoint CreateCheckpoint() => new Checkpoint()
        {
            SchemasToInclude = new[] { "Pttcd" }
        };
    }
}
