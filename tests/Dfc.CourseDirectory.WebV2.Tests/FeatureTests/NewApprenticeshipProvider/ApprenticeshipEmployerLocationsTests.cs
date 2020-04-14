using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider;
using Dfc.CourseDirectory.WebV2.Models;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.NewApprenticeshipProvider
{
    public class ApprenticeshipEmployerLocationsTests : MvcTestBase
    {
        public ApprenticeshipEmployerLocationsTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_HelpdeskUserCannotAccess()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsHelpdesk();

            var mptxInstance = CreateMptxInstance(
                new FlowModel()
                {
                    ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased
                });

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations?providerId={providerId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData(ApprenticeshipQAStatus.Submitted)]
        [InlineData(ApprenticeshipQAStatus.InProgress)]
        [InlineData(ApprenticeshipQAStatus.Passed)]
        [InlineData(ApprenticeshipQAStatus.UnableToComplete)]
        public async Task Get_QAStatusNotValidReturnsBadRequest(ApprenticeshipQAStatus qaStatus)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: qaStatus);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            var mptxInstance = CreateMptxInstance(
                new FlowModel()
                {
                    ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased
                });

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations?providerId={providerId}&ffiid={mptxInstance.InstanceId}");

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

            var mptxInstance = CreateMptxInstance(
                new FlowModel()
                {
                    ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased
                });

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations?providerId={providerId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Get_NoPersistedStateRendersExpectedOutput()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            var mptxInstance = CreateMptxInstance(
                new FlowModel()
                {
                    ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased,
                    ApprenticeshipIsNational = null
                });

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations?providerId={providerId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Null(doc.GetElementById("National").GetAttribute("checked"));
            Assert.Null(doc.GetElementById("National-1").GetAttribute("checked"));
        }

        [Fact]
        public async Task Get_PersistedStateRendersExpectedOutput()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            var mptxInstance = CreateMptxInstance(
                new FlowModel()
                {
                    ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased,
                    ApprenticeshipIsNational = false
                });

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations?providerId={providerId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Null(doc.GetElementById("National").GetAttribute("checked"));
            Assert.Equal("checked", doc.GetElementById("National-1").GetAttribute("checked"));
        }

        [Fact]
        public async Task Post_HelpdeskUserCannotAccess()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsHelpdesk();

            var mptxInstance = CreateMptxInstance(
                new FlowModel()
                {
                    ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased
                });

            var requestContent = new FormUrlEncodedContentBuilder()
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations?providerId={providerId}&ffiid={mptxInstance.InstanceId}",
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

            var mptxInstance = CreateMptxInstance(
                new FlowModel()
                {
                    ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased
                });

            var requestContent = new FormUrlEncodedContentBuilder()
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations?providerId={providerId}&ffiid={mptxInstance.InstanceId}",
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

            var mptxInstance = CreateMptxInstance(
                new FlowModel()
                {
                    ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased
                });

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("National", bool.TrueString)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations?providerId={providerId}&ffiid={mptxInstance.InstanceId}",
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

            var mptxInstance = CreateMptxInstance(
                new FlowModel()
                {
                    ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased
                });

            var requestContent = new FormUrlEncodedContentBuilder()
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations?providerId={providerId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("National", "Enter whether you can deliver this training at employers’ locations anywhere in England");
        }

        [Fact]
        public async Task Post_ValidRequestNationalUpdatesFlowStateAndRedirects()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            var mptxInstance = CreateMptxInstance(
                new FlowModel()
                {
                    ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased
                });

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("National", true)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations?providerId={providerId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            var state = GetMptxInstance<FlowModel>(mptxInstance.InstanceId).State;
            Assert.True(state.ApprenticeshipIsNational);

            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal(
                "/new-apprenticeship-provider/apprenticeship-confirmation",
                UrlHelper.StripQueryParams(response.Headers.Location.OriginalString));
        }

        [Fact]
        public async Task Post_ValidRequestNotNationalUpdatesFlowStateAndRedirects()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            var mptxInstance = CreateMptxInstance(
                new FlowModel()
                {
                    ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased
                });

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("National", false)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations?providerId={providerId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            var state = GetMptxInstance<FlowModel>(mptxInstance.InstanceId).State;
            Assert.False(state.ApprenticeshipIsNational);

            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal(
                "/new-apprenticeship-provider/apprenticeship-employer-locations-regions",
                UrlHelper.StripQueryParams(response.Headers.Location.OriginalString));
        }
    }
}
