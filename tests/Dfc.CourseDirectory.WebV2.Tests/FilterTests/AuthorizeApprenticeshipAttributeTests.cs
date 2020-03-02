using System;
using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FilterTests
{
    public class AuthorizeApprenticeshipAttributeTests : TestBase
    {
        public AuthorizeApprenticeshipAttributeTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
            Factory.HostingOptions.RewriteForbiddenToNotFound = false;
        }

        [Fact]
        public async Task DeveloperUser_CanAccess()
        {
            // Arrange
            var ukprn = 123456;
            await TestData.CreateProvider(ukprn);
            var apprenticeshipId = await TestData.CreateApprenticeship(ukprn);
            User.AsDeveloper();

            // Act
            var response = await HttpClient.GetAsync($"filtertests/authorizeapprenticeshipattribute/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task HelpdeskUser_CanAccess()
        {
            // Arrange
            var ukprn = 123456;
            await TestData.CreateProvider(ukprn);
            var apprenticeshipId = await TestData.CreateApprenticeship(ukprn);
            User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync($"filtertests/authorizeapprenticeshipattribute/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ProviderUserForApprenticeshipsProvider_CanAccess()
        {
            // Arrange
            var ukprn = 123456;
            var providerId = await TestData.CreateProvider(ukprn);
            var apprenticeshipId = await TestData.CreateApprenticeship(ukprn);
            User.AsProviderUser(providerId, Models.ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.GetAsync($"filtertests/authorizeapprenticeshipattribute/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ProviderSuperUserForApprenticeshipsProvider_CanAccess()
        {
            // Arrange
            var ukprn = 123456;
            var providerId = await TestData.CreateProvider(ukprn);
            var apprenticeshipId = await TestData.CreateApprenticeship(ukprn);
            User.AsProviderSuperUser(providerId, Models.ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.GetAsync($"filtertests/authorizeapprenticeshipattribute/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ProviderUserForDifferentProvider_CannotAccess()
        {
            // Arrange
            var ukprn = 123456;
            await TestData.CreateProvider(ukprn);
            var anotherProviderId = await TestData.CreateProvider(ukprn: 23456);
            var apprenticeshipId = await TestData.CreateApprenticeship(ukprn);
            User.AsProviderUser(anotherProviderId, Models.ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.GetAsync($"filtertests/authorizeapprenticeshipattribute/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ProviderSuperUserForDifferentProvider_CannotAccess()
        {
            // Arrange
            var ukprn = 123456;
            await TestData.CreateProvider(ukprn);
            var anotherProviderId = await TestData.CreateProvider(ukprn: 23456);
            var apprenticeshipId = await TestData.CreateApprenticeship(ukprn);
            User.AsProviderSuperUser(anotherProviderId, Models.ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.GetAsync($"filtertests/authorizeapprenticeshipattribute/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    public class AuthorizeApprenticeshipAttributeTestController : Controller
    {
        [HttpGet("filtertests/authorizeapprenticeshipattribute/{apprenticeshipId}")]
        [AuthorizeApprenticeship]
        public IActionResult Get([ApprenticeshipId] Guid apprenticeshipId) => Ok();
    }
}
