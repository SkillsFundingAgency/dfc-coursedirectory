using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Behaviors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.BehaviorTests
{
    public class RequireUserCanAccessCourseBehaviorTests : MvcTestBase
    {
        public RequireUserCanAccessCourseBehaviorTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task AdminUsers_AreNotBlocked(TestUserType userType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            var providerUser = await TestData.CreateUser("provider-user", "user@provider.com", "Test", "User", providerId);
            var courseId = await TestData.CreateCourse(providerId, createdBy: providerUser);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/RequireUserCanAccessCourseBehaviorTests?courseId={courseId}");

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
            var providerId = await TestData.CreateProvider();
            var providerUser = await TestData.CreateUser("provider-user", "user@provider.com", "Test", "User", providerId);
            var courseId = await TestData.CreateCourse(providerId, createdBy: providerUser);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/RequireUserCanAccessCourseBehaviorTests?courseId={courseId}");

            await User.AsTestUser(userType, providerId);

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
            var anotherProviderId = await TestData.CreateProvider(ukprn: 23456);

            var providerId = await TestData.CreateProvider(ukprn: 12345);
            var providerUser = await TestData.CreateUser("provider-user", "user@provider.com", "Test", "User", providerId);
            var courseId = await TestData.CreateCourse(providerId, createdBy: providerUser);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/RequireUserCanAccessCourseBehaviorTests?courseId={courseId}");

            await User.AsTestUser(userType, anotherProviderId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UnauthenticatedUser_IsBlocked()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(ukprn: 12345);
            var providerUser = await TestData.CreateUser("provider-user", "user@provider.com", "Test", "User", providerId);
            var courseId = await TestData.CreateCourse(providerId, createdBy: providerUser);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/RequireUserCanAccessCourseBehaviorTests?courseId={courseId}");

            User.SetNotAuthenticated();

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    [Route("RequireUserCanAccessCourseBehaviorTests")]
    public class RequireUserCanAccessCourseBehaviorTestsController : Controller
    {
        [HttpGet("")]
        [AllowAnonymous]  // Disable the up-front authentication check so our behavior gets executed
        public async Task<IActionResult> Get(
            [FromQuery] RequireUserCanAccessCourseBehaviorTestsRequest request,
            [FromServices] IMediator mediator)
        {
            await mediator.Send(request);
            return Ok();
        }
    }

    public class RequireUserCanAccessCourseBehaviorTestsRequest : IRequest
    {
        public Guid CourseId { get; set; }
    }

    public class RequireUserCanAccessCourseBehaviorTestsHandler :
        IRequestHandler<RequireUserCanAccessCourseBehaviorTestsRequest>,
        IRequireUserCanAccessCourse<RequireUserCanAccessCourseBehaviorTestsRequest>
    {
        public Task<Unit> Handle(RequireUserCanAccessCourseBehaviorTestsRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Unit());
        }

        Guid IRequireUserCanAccessCourse<RequireUserCanAccessCourseBehaviorTestsRequest>.GetCourseId(RequireUserCanAccessCourseBehaviorTestsRequest request) =>
            request.CourseId;
    }
}
