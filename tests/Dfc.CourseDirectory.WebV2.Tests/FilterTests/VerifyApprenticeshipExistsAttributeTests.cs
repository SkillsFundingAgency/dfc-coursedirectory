using System;
using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FilterTests
{
    public class VerifyApprenticeshipExistsAttributeTests : TestBase
    {
        public VerifyApprenticeshipExistsAttributeTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task ApprenticeshipDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var apprenticeshipId = Guid.NewGuid();

            Factory.ProviderOwnershipCache
                .Setup(mock => mock.GetProviderForApprenticeship(apprenticeshipId))
                .ReturnsAsync((int?)null);

            // Act
            var response = await HttpClient.GetAsync($"filtertests/verifyapprenticeshipexistsattribute/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ApprenticeshipDoesExist_ReturnsOk()
        {
            // Arrange
            var apprenticeshipId = Guid.NewGuid();

            Factory.ProviderOwnershipCache
                .Setup(mock => mock.GetProviderForApprenticeship(apprenticeshipId))
                .ReturnsAsync(1234);

            // Act
            var response = await HttpClient.GetAsync($"filtertests/verifyapprenticeshipexistsattribute/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    public class VerifyApprenticeshipExistsAttributeTestController : Controller
    {
        [HttpGet("filtertests/verifyapprenticeshipexistsattribute/{apprenticeshipId}")]
        [VerifyApprenticeshipExists]
        public IActionResult Get(Guid apprenticeshipId) => Ok();
    }
}
