using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FilterTests
{
    public class AuthorizeCourseAttributeTests : MvcTestBase
    {
        public AuthorizeCourseAttributeTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task AdminUsers_AreNotBlocked(TestUserType userType)
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);
            var course = await TestData.CreateCourse(provider.ProviderId, createdBy: providerUser);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/AuthorizeCourseAttributeTests/{course.CourseId}");

            await User.AsTestUser(userType);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task ProviderUsersForSameProviderAsCourse_AreNotBlocked(TestUserType userType)
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);
            var course = await TestData.CreateCourse(provider.ProviderId, createdBy: providerUser);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/AuthorizeCourseAttributeTests/{course.CourseId}");

            await User.AsTestUser(userType, provider.ProviderId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task ProviderUsersForDifferentProviderAsCourse_AreBlocked(TestUserType userType)
        {
            // Arrange
            var anotherProvider = await TestData.CreateProvider();

            var provider = await TestData.CreateProvider();
            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);
            var course = await TestData.CreateCourse(provider.ProviderId, createdBy: providerUser);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/AuthorizeCourseAttributeTests/{course.CourseId}");

            await User.AsTestUser(userType, anotherProvider.ProviderId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UnauthenticatedUser_IsBlocked()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);
            var course = await TestData.CreateCourse(provider.ProviderId, createdBy: providerUser);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/AuthorizeCourseAttributeTests/{course.CourseId}");

            User.SetNotAuthenticated();

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    [Route("AuthorizeCourseAttributeTests/{courseId}")]
    public class AuthorizeCourseAttributeTestsController : Controller
    {
        [HttpGet("")]
        [AllowAnonymous]  // Disable the up-front authentication check so our behavior gets executed
        [AuthorizeCourse]
        public IActionResult Get() => Ok();
    }
}
