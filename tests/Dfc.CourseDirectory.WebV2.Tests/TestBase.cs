using System;
using System.Net.Http;
using Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public abstract class TestBase : IClassFixture<CourseDirectoryApplicationFactory>
    {
        public TestBase(CourseDirectoryApplicationFactory factory)
        {
            Factory = factory;

            factory.ResetMocks();

            HttpClient = factory.CreateClient();
            User.Reset();
            InMemoryDocumentStore.Clear();
            MemoryCache.Clear();
            HostingOptions.RewriteForbiddenToNotFound = true;
        }

        protected CourseDirectoryApplicationFactory Factory { get; }

        protected HostingOptions HostingOptions => Services.GetRequiredService<HostingOptions>();

        protected InMemoryDocumentStore InMemoryDocumentStore => Services.GetRequiredService<InMemoryDocumentStore>();

        protected ClearableMemoryCache MemoryCache => Services.GetRequiredService<IMemoryCache>() as ClearableMemoryCache;

        protected IServiceProvider Services => Factory.Server.Host.Services;

        protected AuthenticatedUserInfo User => Services.GetRequiredService<AuthenticatedUserInfo>();

        protected HttpClient HttpClient { get; }
    }
}
