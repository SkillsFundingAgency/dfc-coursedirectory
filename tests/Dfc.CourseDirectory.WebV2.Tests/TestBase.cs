using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public abstract class TestBase : IClassFixture<CourseDirectoryApplicationFactory>
    {
        public TestBase(CourseDirectoryApplicationFactory factory)
        {
            Factory = factory;

            HttpClient = factory.CreateClient();
            User.Reset();
        }

        protected CourseDirectoryApplicationFactory Factory { get; }

        protected IServiceProvider Services => Factory.Server.Host.Services;

        protected AuthenticatedUserInfo User => Services.GetRequiredService<AuthenticatedUserInfo>();

        protected HttpClient HttpClient { get; }
    }
}
