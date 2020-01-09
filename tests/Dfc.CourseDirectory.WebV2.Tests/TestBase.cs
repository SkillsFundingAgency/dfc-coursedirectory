using System.Net.Http;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public abstract class TestBase : IClassFixture<CourseDirectoryApplicationFactory>
    {
        public TestBase(CourseDirectoryApplicationFactory factory)
        {
            Factory = factory;
        }

        protected CourseDirectoryApplicationFactory Factory { get; }

        protected HttpClient CreateClient => Factory.CreateClient();
    }
}
