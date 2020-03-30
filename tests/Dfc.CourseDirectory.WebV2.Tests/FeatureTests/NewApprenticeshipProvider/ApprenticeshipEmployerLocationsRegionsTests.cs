using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider;
using Dfc.CourseDirectory.WebV2.Models;
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
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsHelpdesk();

            var mptxInstance = CreateMptxInstance(
                "NewApprenticeshipProvider",
                new FlowModel()
                {
                    ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased,
                    ApprenticeshipIsNational = false
                });

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations-regions?providerId={providerId}&ffiid={mptxInstance.InstanceId}");

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
                "NewApprenticeshipProvider",
                new FlowModel()
                {
                    ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased,
                    ApprenticeshipIsNational = false
                });

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations-regions?providerId={providerId}&ffiid={mptxInstance.InstanceId}");

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
                "NewApprenticeshipProvider",
                new FlowModel()
                {
                    ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased,
                    ApprenticeshipIsNational = false
                });

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations-regions?providerId={providerId}&ffiid={mptxInstance.InstanceId}");

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
                "NewApprenticeshipProvider",
                new FlowModel()
                {
                    ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased,
                    ApprenticeshipIsNational = false
                });

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations-regions?providerId={providerId}&ffiid={mptxInstance.InstanceId}");

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
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            var mptxInstance = CreateMptxInstance(
                "NewApprenticeshipProvider",
                new FlowModel()
                {
                    ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased,
                    ApprenticeshipIsNational = false,
                    ApprenticeshipLocationRegionIds = new[] { "E06000001" }  // County Durham
                });

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations-regions?providerId={providerId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();

            // SubRegion in state should be selected
            Assert.Equal("checked", doc.GetElementById("RegionIds-E06000001").GetAttribute("checked"));

            // Parent Region should be expanded
            Assert.Contains(
                "govuk-accordion__section--expanded",
                doc.GetElementById("Region-E12000001").ClassList);
        }

        [Fact]
        public async Task Post_HelpdeskUserCannotAccess()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsHelpdesk();

            var mptxInstance = CreateMptxInstance(
                "NewApprenticeshipProvider",
                new FlowModel()
                {
                    ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased,
                    ApprenticeshipIsNational = false
                });

            var subRegion1Id = Region.All.First().SubRegions.First().Id;
            var subRegion2Id = Region.All.Skip(1).First().SubRegions.First().Id;

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("RegionIds", subRegion1Id)
                .Add("RegionIds", subRegion2Id)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations-regions?providerId={providerId}&ffiid={mptxInstance.InstanceId}",
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
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: qaStatus);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            var mptxInstance = CreateMptxInstance(
                "NewApprenticeshipProvider",
                new FlowModel()
                {
                    ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased,
                    ApprenticeshipIsNational = false
                });

            var subRegion1Id = Region.All.First().SubRegions.First().Id;
            var subRegion2Id = Region.All.Skip(1).First().SubRegions.First().Id;

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("RegionIds", subRegion1Id)
                .Add("RegionIds", subRegion2Id)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations-regions?providerId={providerId}&ffiid={mptxInstance.InstanceId}",
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
                "NewApprenticeshipProvider",
                new FlowModel()
                {
                    ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased,
                    ApprenticeshipIsNational = false
                });

            var subRegion1Id = Region.All.First().SubRegions.First().Id;
            var subRegion2Id = Region.All.Skip(1).First().SubRegions.First().Id;

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("RegionIds", subRegion1Id)
                .Add("RegionIds", subRegion2Id)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations-regions?providerId={providerId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_NoRegionSelectedRendersError()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            var mptxInstance = CreateMptxInstance(
                "NewApprenticeshipProvider",
                new FlowModel()
                {
                    ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased,
                    ApprenticeshipIsNational = false
                });

            var requestContent = new FormUrlEncodedContentBuilder()
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations-regions?providerId={providerId}&ffiid={mptxInstance.InstanceId}",
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
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            var mptxInstance = CreateMptxInstance(
                "NewApprenticeshipProvider",
                new FlowModel()
                {
                    ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased,
                    ApprenticeshipIsNational = false
                });

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("RegionIds", "bad-region-id")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations-regions?providerId={providerId}&ffiid={mptxInstance.InstanceId}",
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
        public async Task Post_ValidRequestUpdatesStateAndRedirects()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            var mptxInstance = CreateMptxInstance(
                "NewApprenticeshipProvider",
                new FlowModel()
                {
                    ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased,
                    ApprenticeshipIsNational = false
                });

            var subRegion1Id = "E06000001";  // County Durham
            var subRegion2Id = "E06000009";  // Blackpool

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("RegionIds", subRegion1Id)
                .Add("RegionIds", subRegion2Id)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations-regions?providerId={providerId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            var state = GetMptxInstance<FlowModel>(mptxInstance.InstanceId).State;
            Assert.False(state.ApprenticeshipIsNational);

            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal(
                "/new-apprenticeship-provider/apprenticeship-confirmation",
                UrlHelper.StripQueryParams(response.Headers.Location.OriginalString));
        }

        [Fact]
        public async Task Post_RegionIdSpecifiedStoresExpandedSubRegionsInState()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            var mptxInstance = CreateMptxInstance(
                "NewApprenticeshipProvider",
                new FlowModel()
                {
                    ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased,
                    ApprenticeshipIsNational = false
                });

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("RegionIds", "E12000001")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-employer-locations-regions?providerId={providerId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            var region = Region.All.Single(r => r.Id == "E12000001");
            var state = GetMptxInstance<FlowModel>(mptxInstance.InstanceId).State;
            Assert.All(region.SubRegions, sr => state.ApprenticeshipLocationRegionIds.Contains(sr.Id));
        }
    }
}
