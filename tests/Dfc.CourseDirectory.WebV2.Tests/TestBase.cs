using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    [Trait("SkipOnCI", "true")]  // Until we have SQL DB on CI
    public abstract class TestBase : IClassFixture<CourseDirectoryApplicationFactory>, IAsyncLifetime
    {
        public TestBase(CourseDirectoryApplicationFactory factory)
        {
            Factory = factory;

            HttpClient = factory.CreateClient();
            Factory.OnTestStarting();
        }

        protected MutableClock Clock => Factory.Clock;

        protected CourseDirectoryApplicationFactory Factory { get; }

        protected HttpClient HttpClient { get; }

        protected TestData TestData => Factory.TestData;

        protected AuthenticatedUserInfo User => Factory.User;

        public Task DisposeAsync() => Task.CompletedTask;

        public Task InitializeAsync() => Factory.OnTestStarted();
    }
}
