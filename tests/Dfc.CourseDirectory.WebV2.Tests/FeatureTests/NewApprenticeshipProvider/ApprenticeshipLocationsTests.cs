using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Models;
using Xunit;
using FlowModel = Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider.FlowModel;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.NewApprenticeshipProvider
{
    public class ApprenticeshipLocationsTests : TestBase
    {
        public ApprenticeshipLocationsTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_HelpdeskUserCannotAccess()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsHelpdesk();

            var mptxInstance = CreateMptxInstance("NewApprenticeshipProvider", new FlowModel());

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-locations?providerId={providerId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData(ApprenticeshipQAStatus.Submitted)]
        [InlineData(ApprenticeshipQAStatus.InProgress)]
        [InlineData(ApprenticeshipQAStatus.Failed)]
        [InlineData(ApprenticeshipQAStatus.Passed)]
        [InlineData(ApprenticeshipQAStatus.UnableToComplete)]
        public async Task Get_QAStatusNotValidReturnsBadRequest(ApprenticeshipQAStatus qaStatus)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: qaStatus);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            var mptxInstance = CreateMptxInstance("NewApprenticeshipProvider", new FlowModel());

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-locations?providerId={providerId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Get_NotApprenticeshipProviderReturnsBadRequest()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted,
                providerType: ProviderType.FE);

            await User.AsProviderUser(providerId, ProviderType.FE);

            var mptxInstance = CreateMptxInstance("NewApprenticeshipProvider", new FlowModel());

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-locations?providerId={providerId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Get_NoPersistedStateRendersExpectedOutput()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            var mptxInstance = CreateMptxInstance("NewApprenticeshipProvider", new FlowModel());

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-locations?providerId={providerId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Null(doc.GetElementById("LocationType").GetAttribute("checked"));
            Assert.Null(doc.GetElementById("LocationType-1").GetAttribute("checked"));
            Assert.Null(doc.GetElementById("LocationType-2").GetAttribute("checked"));
        }

        [Fact]
        public async Task Get_PersistedStateRendersExpectedOutput()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            var mptxInstance = CreateMptxInstance(
                "NewApprenticeshipProvider",
                new FlowModel()
                {
                    ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased
                });

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-locations?providerId={providerId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Null(doc.GetElementById("LocationType").GetAttribute("checked"));
            Assert.Equal("checked", doc.GetElementById("LocationType-1").GetAttribute("checked"));
            Assert.Null(doc.GetElementById("LocationType-2").GetAttribute("checked"));
        }

        [Fact]
        public async Task Post_HelpdeskUserCannotAccess()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsHelpdesk();

            var mptxInstance = CreateMptxInstance("NewApprenticeshipProvider", new FlowModel());

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("LocationType", "EmployerBased")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-locations?providerId={providerId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData(ApprenticeshipQAStatus.Submitted)]
        [InlineData(ApprenticeshipQAStatus.InProgress)]
        [InlineData(ApprenticeshipQAStatus.Failed)]
        [InlineData(ApprenticeshipQAStatus.Passed)]
        [InlineData(ApprenticeshipQAStatus.UnableToComplete)]
        public async Task Post_QAStatusNotValidReturnsBadRequest(ApprenticeshipQAStatus qaStatus)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: qaStatus);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            var mptxInstance = CreateMptxInstance("NewApprenticeshipProvider", new FlowModel());

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("LocationType", "EmployerBased")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-locations?providerId={providerId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_NotApprenticeshipProviderReturnsBadRequest()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted,
                providerType: ProviderType.FE);

            await User.AsProviderUser(providerId, ProviderType.FE);

            var mptxInstance = CreateMptxInstance("NewApprenticeshipProvider", new FlowModel());

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("LocationType", "EmployerBased")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-locations?providerId={providerId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_NoOptionSelectedRendersValidationError()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            var mptxInstance = CreateMptxInstance("NewApprenticeshipProvider", new FlowModel());

            var requestContent = new FormUrlEncodedContentBuilder()
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-locations?providerId={providerId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("LocationType", "Enter where this apprenticeship training will be delivered");
        }

        [Theory]
        [InlineData(ApprenticeshipLocationType.ClassroomBased, "/new-apprenticeship-provider/apprenticeship-classroom-locations")]
        [InlineData(ApprenticeshipLocationType.EmployerBased, "/new-apprenticeship-provider/apprenticeship-employer-locations")]
        [InlineData(ApprenticeshipLocationType.ClassroomBasedAndEmployerBased, "/new-apprenticeship-provider/apprenticeship-mixed-locations")]
        public async Task Post_ValidRequestUpdatesFlowStateAndRedirects(
            ApprenticeshipLocationType locationType,
            string expectedRedirect)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            var mptxInstance = CreateMptxInstance("NewApprenticeshipProvider", new FlowModel());

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("LocationType", locationType.ToString())
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-locations?providerId={providerId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(locationType, GetMptxInstanceState<FlowModel>(mptxInstance.InstanceId).ApprenticeshipLocationType);

            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.StartsWith(expectedRedirect, response.Headers.Location.OriginalString);
        }
    }
}
