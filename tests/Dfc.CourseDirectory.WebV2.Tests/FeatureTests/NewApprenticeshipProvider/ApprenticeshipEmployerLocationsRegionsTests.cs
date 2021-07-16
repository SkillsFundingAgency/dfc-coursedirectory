using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.NewApprenticeshipProvider
{
    public class ApprenticeshipEmployerLocationsRegionsTests : MvcTestBase
    {
        public ApprenticeshipEmployerLocationsRegionsTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_HelpdeskUserCannotAccess()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsHelpdesk();

            var flowModel = new FlowModel();
            flowModel.SetApprenticeshipLocationType(ApprenticeshipLocationType.EmployerBased);
            flowModel.SetApprenticeshipIsNational(false);
            var mptxInstance = CreateMptxInstance(flowModel);

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations-regions?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}");

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
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: qaStatus);

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetApprenticeshipLocationType(ApprenticeshipLocationType.EmployerBased);
            flowModel.SetApprenticeshipIsNational(false);
            var mptxInstance = CreateMptxInstance(flowModel);

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations-regions?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Get_NotApprenticeshipProviderReturnsForbidden()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted,
                providerType: ProviderType.FE);

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            var flowModel = new FlowModel();
            flowModel.SetApprenticeshipLocationType(ApprenticeshipLocationType.EmployerBased);
            flowModel.SetApprenticeshipIsNational(false);
            var mptxInstance = CreateMptxInstance(flowModel);

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations-regions?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Get_NoPersistedStateRendersExpectedOutput()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetApprenticeshipLocationType(ApprenticeshipLocationType.EmployerBased);
            flowModel.SetApprenticeshipIsNational(false);
            var mptxInstance = CreateMptxInstance(flowModel);

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations-regions?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            var checkboxes = doc.GetElementsByClassName("govuk-checkboxes__input");
            Assert.All(checkboxes, cb => Assert.Null(cb.GetAttribute("checked")));
        }

        [Fact]
        public async Task Get_PersistedStateRendersExpectedOutput()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetApprenticeshipLocationType(ApprenticeshipLocationType.EmployerBased);
            flowModel.SetApprenticeshipIsNational(false);
            flowModel.SetApprenticeshipLocationRegionIds(new[] { "E06000001" });  // County Durham
            var mptxInstance = CreateMptxInstance(flowModel);

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations-regions?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();

            // SubRegion in state should be selected
            Assert.Equal("checked", doc.GetElementById("RegionIds-E06000001").GetAttribute("checked"));

            // Parent Region should be expanded
            Assert.Contains(
                "govuk-accordion__section--expanded",
                doc.GetElementByTestId("RegionIds-E12000001").ClassList);
        }

        [Fact]
        public async Task Post_HelpdeskUserCannotAccess()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsHelpdesk();

            var flowModel = new FlowModel();
            flowModel.SetApprenticeshipLocationType(ApprenticeshipLocationType.EmployerBased);
            flowModel.SetApprenticeshipIsNational(false);
            var mptxInstance = CreateMptxInstance(flowModel);

            var regions = await RegionCache.GetAllRegions();
            var subRegion1Id = regions.First().SubRegions.First().Id;
            var subRegion2Id = regions.Skip(1).First().SubRegions.First().Id;

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("RegionIds", subRegion1Id)
                .Add("RegionIds", subRegion2Id)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations-regions?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData(ApprenticeshipQAStatus.Submitted)]
        [InlineData(ApprenticeshipQAStatus.InProgress)]
        [InlineData(ApprenticeshipQAStatus.Passed)]
        [InlineData(ApprenticeshipQAStatus.UnableToComplete)]
        public async Task Post_QAStatusNotValidReturnsBadRequest(ApprenticeshipQAStatus qaStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: qaStatus);

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetApprenticeshipLocationType(ApprenticeshipLocationType.EmployerBased);
            flowModel.SetApprenticeshipIsNational(false);
            var mptxInstance = CreateMptxInstance(flowModel);

            var regions = await RegionCache.GetAllRegions();
            var subRegion1Id = regions.First().SubRegions.First().Id;
            var subRegion2Id = regions.Skip(1).First().SubRegions.First().Id;

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("RegionIds", subRegion1Id)
                .Add("RegionIds", subRegion2Id)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations-regions?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_NotApprenticeshipProviderReturnsForbidden()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted,
                providerType: ProviderType.FE);

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            var flowModel = new FlowModel();
            flowModel.SetApprenticeshipLocationType(ApprenticeshipLocationType.EmployerBased);
            flowModel.SetApprenticeshipIsNational(false);
            var mptxInstance = CreateMptxInstance(flowModel);

            var regions = await RegionCache.GetAllRegions();
            var subRegion1Id = regions.First().SubRegions.First().Id;
            var subRegion2Id = regions.Skip(1).First().SubRegions.First().Id;

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("RegionIds", subRegion1Id)
                .Add("RegionIds", subRegion2Id)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations-regions?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Post_NoRegionSelectedRendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetApprenticeshipLocationType(ApprenticeshipLocationType.EmployerBased);
            flowModel.SetApprenticeshipIsNational(false);
            var mptxInstance = CreateMptxInstance(flowModel);

            var requestContent = new FormUrlEncodedContentBuilder()
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations-regions?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            var errorSummary = doc.GetElementsByClassName("govuk-error-summary__list").First();
            Assert.Contains(
                errorSummary.GetElementsByTagName("a"),
                e => e.TextContent.Trim() == "Select at least one sub-region");
        }

        [Fact]
        public async Task Post_InvalidRegionIdRendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetApprenticeshipLocationType(ApprenticeshipLocationType.EmployerBased);
            flowModel.SetApprenticeshipIsNational(false);
            var mptxInstance = CreateMptxInstance(flowModel);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("RegionIds", "bad-region-id")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations-regions?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            var errorSummary = doc.GetElementsByClassName("govuk-error-summary__list").First();
            Assert.Contains(
                errorSummary.GetElementsByTagName("a"),
                e => e.TextContent.Trim() == "Select at least one sub-region");
        }

        [Theory]
        [InlineData(ApprenticeshipLocationType.EmployerBased, false, "/new-apprenticeship-provider/apprenticeship-confirmation")]
        [InlineData(ApprenticeshipLocationType.ClassroomBasedAndEmployerBased, false, "/new-apprenticeship-provider/add-apprenticeship-classroom-location")]
        [InlineData(ApprenticeshipLocationType.ClassroomBasedAndEmployerBased, true, "/new-apprenticeship-provider/apprenticeship-confirmation")]
        public async Task Post_ValidRequestUpdatesStateAndRedirects(
            ApprenticeshipLocationType locationType,
            bool gotClassroomLocation,
            string expectedRedirectLocation)
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetApprenticeshipLocationType(locationType);
            flowModel.SetApprenticeshipIsNational(false);
            var mptxInstance = CreateMptxInstance(flowModel);

            if (gotClassroomLocation)
            {
            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo())).VenueId;

                mptxInstance.Update(s => s.SetClassroomLocationForVenue(
                    venueId,
                    originalVenueId: null,
                    radius: 5,
                    new[] { ApprenticeshipDeliveryMode.BlockRelease }));
            }

            var subRegion1Id = "E06000001";  // County Durham
            var subRegion2Id = "E06000009";  // Blackpool

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("RegionIds", subRegion1Id)
                .Add("RegionIds", subRegion2Id)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations-regions?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            var state = GetMptxInstance<FlowModel>(mptxInstance.InstanceId).State;
            Assert.False(state.ApprenticeshipIsNational);

            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal(
                expectedRedirectLocation,
                UrlHelper.StripQueryParams(response.Headers.Location.OriginalString));
        }

        [Fact]
        public async Task Post_RegionIdSpecifiedStoresExpandedSubRegionsInState()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetApprenticeshipLocationType(ApprenticeshipLocationType.EmployerBased);
            flowModel.SetApprenticeshipIsNational(false);
            var mptxInstance = CreateMptxInstance(flowModel);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("RegionIds", "E12000001")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations-regions?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            var regions = await RegionCache.GetAllRegions();
            var region = regions.Single(r => r.Id == "E12000001");
            var state = GetMptxInstance<FlowModel>(mptxInstance.InstanceId).State;
            Assert.All(region.SubRegions, sr => state.ApprenticeshipLocationSubRegionIds.Contains(sr.Id));
        }
    }
}
