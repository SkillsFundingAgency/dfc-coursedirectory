using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Dfc.CourseDirectory.FindACourseApi.Tests
{
    [Collection("FindACourseApi")]
    public abstract class TestBase : IClassFixture<WebApplicationFactory<Startup>>
    {
        public TestBase(FindACourseApiApplicationFactory factory)
        {
            Factory = factory;
            HttpClient = factory.CreateClient();
        }

        public HttpClient HttpClient { get; }

        public FindACourseApiApplicationFactory Factory { get; }
    }
}
