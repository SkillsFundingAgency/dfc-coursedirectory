using System;
using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FilterTests
{
    public class VerifyApprenticeshipIdAttributeTests : TestBase
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
            var ukprn = 12345;
            await TestData.CreateProvider(ukprn);
            var apprenticeshipId = await TestData.CreateApprenticeship(ukprn);

            // Act
            var response = await HttpClient.GetAsync($"filtertests/verifyapprenticeshipidattributetests/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task UnboundProviderInfo_IsPopulated()
        {
            // Arrange
            var ukprn = 1234;
            await TestData.CreateProvider(ukprn);
            var apprenticeshipId = await TestData.CreateApprenticeship(ukprn);
            await User.AsDeveloper();  // Ensure UKPRN doesn't get bound from authentication ticket

            // Act
            var response = await HttpClient.GetAsync($"filtertests/verifyapprenticeshipidattributetests/withproviderinfo/{apprenticeshipId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseJson = JToken.Parse(await response.Content.ReadAsStringAsync());
            Assert.True(int.TryParse(responseJson["ukprn"].ToString(), out var boundUkprn), "Binding failed.");
            Assert.Equal(ukprn, boundUkprn);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task MismatchingProviderInfoUkprnAndApprenticeshipUkprn_ReturnsError(TestUserType userType)
        {
            // Arrange
            var ukprn = 1234;
            var providerId = await TestData.CreateProvider(ukprn);
            var anotherProviderId = await TestData.CreateProvider(ukprn: 56789);
            var apprenticeshipId = await TestData.CreateApprenticeship(ukprn);
            await User.AsTestUser(userType, anotherProviderId);

            // Act
            var response = await HttpClient.GetAsync(
                $"filtertests/verifyapprenticeshipidattributetests/withproviderinfo/{apprenticeshipId}?providerId={anotherProviderId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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

        [HttpGet("filtertests/verifyapprenticeshipidattributetests/withproviderinfo/{apprenticeshipId}")]
        public IActionResult GetWithProviderInfo([ApprenticeshipId] Guid apprenticeshipId, ProviderInfo providerInfo) => Json(providerInfo);

        [HttpGet("filtertests/verifyapprenticeshipidattributetests/overridenstatus/{apprenticeshipId}")]
        public IActionResult GetWithOverridenStatus([ApprenticeshipId(DoesNotExistResponseStatusCode = 400)] Guid apprenticeshipId) => Ok();
    }
}
