using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.ChooseQualification;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.ChooseQualification
{
    public class CheckAndPublishTests : MvcTestBase
    {

        public CheckAndPublishTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        //Navigating directly renders return error
        [Fact]
        private async Task Get_CheckAndPublishDirectly_ReturnsError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);

            // Act
            var get = await HttpClient.GetAsync(
                 $"/courses/add/check-and-publish");

            // Assert
            get.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // Navigate directly without Mptx ID renders return error
        [Fact]
        private async Task Get_CheckAndPublishWithoutMptxId_ReturnsError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            MptxManager.CreateInstance(new FlowModel());
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);

            // Act
            var get = await HttpClient.GetAsync(
                $"/courses/add/check-and-publishproviderId={provider.ProviderId}");

            // Assert
            get.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // check the headinging is correct


    }
}
