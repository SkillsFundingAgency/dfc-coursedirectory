﻿using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.Core.Attributes;
using Dfc.CourseDirectory.Web.Tests.Core;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.Web.Tests.FilterTests
{
    public class RedirectToProviderSelectionActionFilterTests : MvcTestBase
    {
        public RedirectToProviderSelectionActionFilterTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task ActionDecoratedWithRequireProviderContext_ReturnsRedirectToSelectProviderView()
        {
            // Arrange
            await User.AsDeveloper();

            // Act
            var response = await HttpClient.GetAsync(
                "RedirectToProviderSelectionActionFilterTest/without-parameter");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("/provider-search", UrlHelper.StripQueryParams(response.Headers.Location.OriginalString));
        }
    }

    public class RedirectToProviderSelectionActionFilterTestController : Controller
    {
        [HttpGet("RedirectToProviderSelectionActionFilterTest/without-parameter")]
        [RequireProviderContext]
        public IActionResult GetWithoutParameter() => Ok("Yay");
    }
}
