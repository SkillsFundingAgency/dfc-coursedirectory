using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Cookies
{
    public class AcceptCookiesTest : MvcTestBase
    {
        public AcceptCookiesTest(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Post_ReturnsOk()
        {
            // Arrange

            // Act
            var response = await HttpClient.PostAsync("cookies/acceptcookies", null);
            var doc = await response.GetDocument();

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Null(doc.GetElementById("cookiesform"));
        }
    }
}
