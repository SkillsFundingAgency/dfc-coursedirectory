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
    public class RequireUserIsAdminTests : MvcTestBase
    {
        public RequireUserIsAdminTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task AdminUsers_AreNotBlocked(TestUserType userType)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "/RequireUserIsAdminTests");

            await User.AsTestUser(userType);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task ProviderUsers_AreBlocked(TestUserType userType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider();

            var request = new HttpRequestMessage(HttpMethod.Get, "/RequireUserIsAdminTests");

            await User.AsTestUser(userType, providerId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    [Route("RequireUserIsAdminTests")]
    public class RequireUserIsAdminTestsController : Controller
    {
        [HttpGet("")]
        [AllowAnonymous]  // Disable the up-front authentication check so our behavior gets executed
        public async Task<IActionResult> Get([FromServices] IMediator mediator)
        {
            await mediator.Send(new RequireUserIsAdminTestsRequest());
            return Ok();
        }
    }

    public class RequireUserIsAdminTestsRequest : IRequest
    {
    }

    public class RequireUserIsAdminTestsHandler :
        IRequestHandler<RequireUserIsAdminTestsRequest>,
        IRequireUserIsAdmin<RequireUserIsAdminTestsRequest>
    {
        public Task<Unit> Handle(RequireUserIsAdminTestsRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Unit());
        }
    }
}
