using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests
{
    public class ErrorTests : IClassFixture<ErrorTestFixture>
    {
        public ErrorTests(ErrorTestFixture fixture)
        {
            HttpClient = fixture.CreateClient(new WebApplicationFactoryClientOptions()
            {
                AllowAutoRedirect = false
            });
        }

        public HttpClient HttpClient { get; }

        [Fact]
        public async Task UnhandledException_ReturnsErrorView()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("errortests");

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("Sorry, there is a problem with the service", doc.QuerySelector("h1").TextContent);
        }

        [Fact]
        public async Task RequestingErrorUrlDirectly_ReturnsNotFound()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("error");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task NotFound_ReturnsNotFoundView()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("notarealurl");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("Sorry, there is a problem with the service", doc.QuerySelector("h1").TextContent);
        }

        [Fact]
        public async Task Forbidden_ReturnsNotFoundView()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("errortests/forbidden");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("Sorry, there is a problem with the service", doc.QuerySelector("h1").TextContent);
        }
    }

    [AllowAnonymous]
    public class ErrorTestController : Controller
    {
        [HttpGet("errortests")]
        public IActionResult Get() => throw new Exception("Bang!");

        [HttpGet("errortests/forbidden")]
        public IActionResult Forbidden() => Forbid();
    }

    public class ErrorTestFixture : CourseDirectoryApplicationFactory
    {
        public ErrorTestFixture(IMessageSink messageSink)
            : base(messageSink)
        {
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IStartupFilter, AddCourseDirectoryErrorHandlingFilter>();
        }

        private class AddCourseDirectoryErrorHandlingFilter : IStartupFilter
        {
            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            {
                return app =>
                {
                    app.UseCourseDirectoryErrorHandling();

                    next(app);
                };
            }
        }
    }
}
