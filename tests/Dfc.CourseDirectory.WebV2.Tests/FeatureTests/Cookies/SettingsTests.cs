using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Testing;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Cookies
{
    public class SettingsTests : MvcTestBase
    {
        public SettingsTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
            HttpClient = factory.CreateClient(new WebApplicationFactoryClientOptions()
            {
                AllowAutoRedirect = true
            });
        }

        [Fact]
        public async Task Post_MissingAllowAnalyticsCookies_RendersError()
        {
            // Arrange
            var requestContent = new FormUrlEncodedContentBuilder()
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync("/cookies/settings", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("AllowAnalyticsCookies", "Select if you want to use cookies that measure website use");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Post_ValidRequest_SetsPreferencesAndShowsNotification(bool allowAnalyticsCookies)
        {
            // Arrange
            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("AllowAnalyticsCookies", allowAnalyticsCookies)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync("/cookies/settings", requestContent);

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            Assert.NotNull(doc.GetElementByTestId("saved-notification"));
        }
    }
}
