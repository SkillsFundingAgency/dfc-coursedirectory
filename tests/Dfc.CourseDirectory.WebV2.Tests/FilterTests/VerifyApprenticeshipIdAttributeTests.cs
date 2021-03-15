using System;
using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FilterTests
{
    public class VerifyApprenticeshipIdAttributeTests : MvcTestBase
    {
        public VerifyApprenticeshipIdAttributeTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task ApprenticeshipDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var apprenticeshipId = Guid.NewGuid();

            // Act
            var response = await HttpClient.GetAsync($"filtertests/verifyapprenticeshipidattributetests/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ApprenticeshipDoesExist_ReturnsOk()
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");
            var apprenticeshipId = (await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo())).Id;

            // Act
            var response = await HttpClient.GetAsync($"filtertests/verifyapprenticeshipidattributetests/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ApprenticeshipDoesNotExistWithOverridenStatusCode_ReturnsStatusCode()
        {
            // Arrange
            var apprenticeshipId = Guid.NewGuid();

            // Act
            var response = await HttpClient.GetAsync($"filtertests/verifyapprenticeshipidattributetests/overridenstatus/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    public class ApprenticeshipIdAttributeTestController : Controller
    {
        [HttpGet("filtertests/verifyapprenticeshipidattributetests/{apprenticeshipId}")]
        public IActionResult Get([ApprenticeshipId] Guid apprenticeshipId) => Ok();

        [HttpGet("filtertests/verifyapprenticeshipidattributetests/overridenstatus/{apprenticeshipId}")]
        public IActionResult GetWithOverridenStatus([ApprenticeshipId(DoesNotExistResponseStatusCode = 400)] Guid apprenticeshipId) => Ok();
    }
}
