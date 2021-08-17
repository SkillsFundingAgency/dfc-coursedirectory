using System;
using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Filters;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FilterTests
{
    public class AuthorizeApprenticeshipAttributeTests : MvcTestBase
    {
        public AuthorizeApprenticeshipAttributeTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task DeveloperUser_CanAccess()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");
            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: User.ToUserInfo())).ApprenticeshipId;
            await User.AsDeveloper();

            // Act
            var response = await HttpClient.GetAsync($"filtertests/authorizeapprenticeshipattribute/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task HelpdeskUser_CanAccess()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");
            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: User.ToUserInfo())).ApprenticeshipId;
            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync($"filtertests/authorizeapprenticeshipattribute/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ProviderUserForApprenticeshipsProvider_CanAccess()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");
            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: User.ToUserInfo())).ApprenticeshipId;
            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.GetAsync($"filtertests/authorizeapprenticeshipattribute/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ProviderSuperUserForApprenticeshipsProvider_CanAccess()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");
            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: User.ToUserInfo())).ApprenticeshipId;
            await User.AsProviderSuperUser(provider.ProviderId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.GetAsync($"filtertests/authorizeapprenticeshipattribute/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ProviderUserForDifferentProvider_CannotAccess()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var anotherProvider = await TestData.CreateProvider();
            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");
            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: User.ToUserInfo())).ApprenticeshipId;
            await User.AsProviderUser(anotherProvider.ProviderId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.GetAsync($"filtertests/authorizeapprenticeshipattribute/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ProviderSuperUserForDifferentProvider_CannotAccess()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var anotherProvider = await TestData.CreateProvider();
            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");
            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: User.ToUserInfo())).ApprenticeshipId;
            await User.AsProviderSuperUser(anotherProvider.ProviderId, ProviderType.Apprenticeships);

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
