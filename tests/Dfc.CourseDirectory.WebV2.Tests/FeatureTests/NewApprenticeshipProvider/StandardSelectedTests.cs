using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.NewApprenticeshipProvider
{
    public class StandardSelectedTests : MvcTestBase
    {
        public StandardSelectedTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_StoresPassedStandardInState()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var standard = await TestData.CreateStandard(standardCode: 123, version: 1, standardName: "My standard");

            var flowModel = new FlowModel();
            flowModel.SetApprenticeshipStandard(standard);
            var mptxInstance = CreateMptxInstance(flowModel);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"new-apprenticeship-provider/standard-selected?" +
                $"providerId={provider.ProviderId}&" +
                $"ffiid={mptxInstance.InstanceId}&" +
                $"standardCode=123&version=1");

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should().Be(
                $"/new-apprenticeship-provider/apprenticeship-details?ffiid={mptxInstance.InstanceId}&providerId={provider.ProviderId}");

            using (new AssertionScope())
            {
                mptxInstance.State.ApprenticeshipStandard.StandardCode.Should().Be(123);
                mptxInstance.State.ApprenticeshipStandard.Version.Should().Be(1);
            }
        }
    }
}
