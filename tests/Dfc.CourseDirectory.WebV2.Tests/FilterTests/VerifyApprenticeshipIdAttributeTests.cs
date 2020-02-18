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

            Factory.ProviderOwnershipCache
                .Setup(mock => mock.GetProviderForApprenticeship(apprenticeshipId))
                .ReturnsAsync((int?)null);

            // Act
            var response = await HttpClient.GetAsync($"filtertests/verifyapprenticeshipidattributetests/{apprenticeshipId}");

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
            var response = await HttpClient.GetAsync($"filtertests/verifyapprenticeshipidattributetests/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task UnboundProviderInfo_IsPopulated()
        {
            // Arrange
            User.AsDeveloper();  // Ensure UKPRN doesn't get bound from authentication ticket

            var apprenticeshipId = Guid.NewGuid();
            var providerId = Guid.NewGuid();
            var ukprn = 1234;

            Factory.ProviderOwnershipCache
                .Setup(mock => mock.GetProviderForApprenticeship(apprenticeshipId))
                .ReturnsAsync(ukprn);

            Factory.ProviderInfoCache
                .Setup(mock => mock.GetProviderInfo(ukprn))
                .ReturnsAsync(new ProviderInfo()
                {
                    ProviderId = providerId,
                    Ukprn = ukprn
                });

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
            var anotherUkprn = 56789;
            User.AsTestUser(userType, anotherUkprn);

            var apprenticeshipId = Guid.NewGuid();
            var ukprn = 1234;

            Factory.ProviderOwnershipCache
                .Setup(mock => mock.GetProviderForApprenticeship(apprenticeshipId))
                .ReturnsAsync(ukprn);

            Factory.ProviderInfoCache
                .Setup(mock => mock.GetProviderInfo(anotherUkprn))
                .ReturnsAsync(new ProviderInfo()
                {
                    ProviderId = Guid.NewGuid(),
                    Ukprn = anotherUkprn
                });

            // Act
            var response = await HttpClient.GetAsync($"filtertests/verifyapprenticeshipidattributetests/withproviderinfo/{apprenticeshipId}?ukprn=56789");

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
    }
}
