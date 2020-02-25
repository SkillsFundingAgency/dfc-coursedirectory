using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FilterTests
{
    public class RestrictApprenticeshipQAStatusAttributeTests : TestBase
    {
        public RestrictApprenticeshipQAStatusAttributeTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(ApprenticeshipQAStatus.InProgress)]
        [InlineData(ApprenticeshipQAStatus.Passed)]
        [InlineData(ApprenticeshipQAStatus.Submitted)]
        public async Task StatusNotInPermittedSet_ReturnsBadRequest(ApprenticeshipQAStatus status)
        {
            // Arrange
            var ukprn = 12345;

            await TestData.CreateProvider(ukprn, apprenticeshipQAStatus: status);

            // Act
            var response = await HttpClient.GetAsync($"restrictapprenticeshipqastatusattributetests?ukprn={ukprn}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(ApprenticeshipQAStatus.NotStarted)]
        [InlineData(ApprenticeshipQAStatus.Failed)]
        public async Task StatusInPermittedSet_ReturnsOk(ApprenticeshipQAStatus status)
        {
            // Arrange
            var ukprn = 12345;

            await TestData.CreateProvider(ukprn, apprenticeshipQAStatus: status);

            // Act
            var response = await HttpClient.GetAsync($"restrictapprenticeshipqastatusattributetests?ukprn={ukprn}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    public class RestrictApprenticeshipQAStatusAttributeTestsController : Controller
    {
        [HttpGet("restrictapprenticeshipqastatusattributetests")]
        [RestrictApprenticeshipQAStatus(ApprenticeshipQAStatus.NotStarted, ApprenticeshipQAStatus.Failed)]
        public IActionResult Get(ProviderInfo provider) => Ok();
    }
}
