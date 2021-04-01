using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider;
using Moq;
using OneOf.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.NewApprenticeshipProvider
{
    public class ApprenticeshipSummaryTests : MvcTestBase
    {
        public ApprenticeshipSummaryTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_HelpdeskUser_ReturnsForbidden()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var standardCode = 123;
            var standardVersion = 1;
            var standard = await TestData.CreateStandard(standardCode, standardVersion, standardName: "My standard");

            await User.AsHelpdesk();

            var flowModel = new FlowModel();
            flowModel.SetProviderDetails("Provider 1 rocks");
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            flowModel.SetApprenticeshipDetails(
                marketingInformation: "My apprenticeship",
                website: "http://provider.com/apprenticeship",
                contactTelephone: "01234 5678902",
                contactEmail: "guy@provider.com",
                contactWebsite: "http://provider.com");
            flowModel.SetApprenticeshipLocationType(ApprenticeshipLocationType.EmployerBased);
            flowModel.SetApprenticeshipIsNational(true);
            var mptxInstance = CreateMptxInstance(flowModel);

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData(ApprenticeshipQAStatus.Submitted)]
        [InlineData(ApprenticeshipQAStatus.InProgress)]
        [InlineData(ApprenticeshipQAStatus.Passed)]
        [InlineData(ApprenticeshipQAStatus.UnableToComplete)]
        public async Task Get_QAStatusNotValid_ReturnsBadRequest(ApprenticeshipQAStatus qaStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: qaStatus);

            var standardCode = 123;
            var standardVersion = 1;
            var standard = await TestData.CreateStandard(standardCode, standardVersion, standardName: "My standard");

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetProviderDetails("Provider 1 rocks");
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            flowModel.SetApprenticeshipDetails(
                marketingInformation: "My apprenticeship",
                website: "http://provider.com/apprenticeship",
                contactTelephone: "01234 5678902",
                contactEmail: "guy@provider.com",
                contactWebsite: "http://provider.com");
            flowModel.SetApprenticeshipLocationType(ApprenticeshipLocationType.EmployerBased);
            flowModel.SetApprenticeshipIsNational(true);
            var mptxInstance = CreateMptxInstance(flowModel);

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Get_NotApprenticeshipProvider_ReturnsForbidden()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted,
                providerType: ProviderType.FE);

            var standardCode = 123;
            var standardVersion = 1;
            var standard = await TestData.CreateStandard(standardCode, standardVersion, standardName: "My standard");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            var flowModel = new FlowModel();
            flowModel.SetProviderDetails("Provider 1 rocks");
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            flowModel.SetApprenticeshipDetails(
                marketingInformation: "My apprenticeship",
                website: "http://provider.com/apprenticeship",
                contactTelephone: "01234 5678902",
                contactEmail: "guy@provider.com",
                contactWebsite: "http://provider.com");
            flowModel.SetApprenticeshipLocationType(ApprenticeshipLocationType.EmployerBased);
            flowModel.SetApprenticeshipIsNational(true);
            var mptxInstance = CreateMptxInstance(flowModel);

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Get_IncompleteFlow_ReturnsBadRequest()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            var mptxInstance = CreateMptxInstance(flowModel);

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Get_MissingClassroomLocations_RendersExpectedOutput()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var standardCode = 123;
            var standardVersion = 1;
            var standard = await TestData.CreateStandard(standardCode, standardVersion, standardName: "My standard");

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetProviderDetails("Provider 1 rocks");
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            flowModel.SetApprenticeshipDetails(
                marketingInformation: "My apprenticeship",
                website: "http://provider.com/apprenticeship",
                contactTelephone: "01234 5678902",
                contactEmail: "guy@provider.com",
                contactWebsite: "http://provider.com");
            flowModel.SetApprenticeshipLocationType(ApprenticeshipLocationType.ClassroomBased);
            var mptxInstance = CreateMptxInstance(flowModel);

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.NotNull(doc.GetElementsByClassName("govuk-error-summary"));
        }

        [Fact]
        public async Task Get_ValidRequest_RendersExpectedOutput()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var standardCode = 123;
            var standardVersion = 1;
            var standard = await TestData.CreateStandard(standardCode, standardVersion, standardName: "My standard");

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetProviderDetails("Provider 1 rocks");
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            flowModel.SetApprenticeshipDetails(
                marketingInformation: "My apprenticeship",
                website: "http://provider.com/apprenticeship",
                contactTelephone: "01234 5678902",
                contactEmail: "guy@provider.com",
                contactWebsite: "http://provider.com");
            flowModel.SetApprenticeshipLocationType(ApprenticeshipLocationType.EmployerBased);
            flowModel.SetApprenticeshipIsNational(true);
            var mptxInstance = CreateMptxInstance(flowModel);

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("http://provider.com/apprenticeship", doc.GetSummaryListValueWithKey("Apprenticeship website"));
            Assert.Equal("guy@provider.com", doc.GetSummaryListValueWithKey("Email"));
            Assert.Equal("01234 5678902", doc.GetSummaryListValueWithKey("Telephone"));
            Assert.Equal("http://provider.com", doc.GetSummaryListValueWithKey("Contact URL"));
        }

        [Fact]
        public async Task Post_HelpdeskUser_ReturnsForbidden()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var standardCode = 123;
            var standardVersion = 1;
            var standard = await TestData.CreateStandard(standardCode, standardVersion, standardName: "My standard");

            await User.AsHelpdesk();

            var flowModel = new FlowModel();
            flowModel.SetProviderDetails("Provider 1 rocks");
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            flowModel.SetApprenticeshipDetails(
                marketingInformation: "My apprenticeship",
                website: "http://provider.com/apprenticeship",
                contactTelephone: "01234 5678902",
                contactEmail: "guy@provider.com",
                contactWebsite: "http://provider.com");
            flowModel.SetApprenticeshipLocationType(ApprenticeshipLocationType.EmployerBased);
            flowModel.SetApprenticeshipIsNational(true);
            var mptxInstance = CreateMptxInstance(flowModel);

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData(ApprenticeshipQAStatus.Submitted)]
        [InlineData(ApprenticeshipQAStatus.InProgress)]
        [InlineData(ApprenticeshipQAStatus.Passed)]
        [InlineData(ApprenticeshipQAStatus.UnableToComplete)]
        public async Task Post_QAStatusNotValid_ReturnsBadRequest(ApprenticeshipQAStatus qaStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: qaStatus);

            var standardCode = 123;
            var standardVersion = 1;
            var standard = await TestData.CreateStandard(standardCode, standardVersion, standardName: "My standard");

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetProviderDetails("Provider 1 rocks");
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            flowModel.SetApprenticeshipDetails(
                marketingInformation: "My apprenticeship",
                website: "http://provider.com/apprenticeship",
                contactTelephone: "01234 5678902",
                contactEmail: "guy@provider.com",
                contactWebsite: "http://provider.com");
            flowModel.SetApprenticeshipLocationType(ApprenticeshipLocationType.EmployerBased);
            flowModel.SetApprenticeshipIsNational(true);
            var mptxInstance = CreateMptxInstance(flowModel);

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_NotApprenticeshipProvider_ReturnsForbidden()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted,
                providerType: ProviderType.FE);

            var standardCode = 123;
            var standardVersion = 1;
            var standard = await TestData.CreateStandard(standardCode, standardVersion, standardName: "My standard");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            var flowModel = new FlowModel();
            flowModel.SetProviderDetails("Provider 1 rocks");
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            flowModel.SetApprenticeshipDetails(
                marketingInformation: "My apprenticeship",
                website: "http://provider.com/apprenticeship",
                contactTelephone: "01234 5678902",
                contactEmail: "guy@provider.com",
                contactWebsite: "http://provider.com");
            flowModel.SetApprenticeshipLocationType(ApprenticeshipLocationType.EmployerBased);
            flowModel.SetApprenticeshipIsNational(true);
            var mptxInstance = CreateMptxInstance(flowModel);

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Post_IncompleteFlow_ReturnsBadRequest()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            var mptxInstance = CreateMptxInstance(flowModel);

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_MissingClassroomLocations_RendersErrorMessage()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var standardCode = 123;
            var standardVersion = 1;
            var standard = await TestData.CreateStandard(standardCode, standardVersion, standardName: "My standard");

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetProviderDetails("Provider 1 rocks");
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            flowModel.SetApprenticeshipDetails(
                marketingInformation: "My apprenticeship",
                website: "http://provider.com/apprenticeship",
                contactTelephone: "01234 5678902",
                contactEmail: "guy@provider.com",
                contactWebsite: "http://provider.com");
            flowModel.SetApprenticeshipLocationType(ApprenticeshipLocationType.ClassroomBased);
            var mptxInstance = CreateMptxInstance(flowModel);

            Guid apprenticeshipId = default;
            CosmosDbQueryDispatcher.Callback<CreateApprenticeship, Success>(q => apprenticeshipId = q.Id);

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.NotNull(doc.GetElementsByClassName("govuk-error-summary"));
        }

        [Fact]
        public async Task PostConfirmation_ValidRequestWithNationalLocations_CreatesApprenticeshipQASubmissionUpdatesQAStatusAndReturnsConfirmation()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var standardCode = 123;
            var standardVersion = 1;
            var standard = await TestData.CreateStandard(standardCode, standardVersion, standardName: "My standard");

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetProviderDetails("Provider 1 rocks");
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            flowModel.SetApprenticeshipDetails(
                marketingInformation: "My apprenticeship",
                website: "http://provider.com/apprenticeship",
                contactTelephone: "01234 5678902",
                contactEmail: "guy@provider.com",
                contactWebsite: "http://provider.com");
            flowModel.SetApprenticeshipLocationType(ApprenticeshipLocationType.EmployerBased);
            flowModel.SetApprenticeshipIsNational(true);
            var mptxInstance = CreateMptxInstance(flowModel);

            Guid apprenticeshipId = default;
            CosmosDbQueryDispatcher.Callback<CreateApprenticeship, Success>(q => apprenticeshipId = q.Id);

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            CosmosDbQueryDispatcher.Verify(mock => mock.ExecuteQuery(It.Is<CreateApprenticeship>(q =>
                q.ApprenticeshipTitle == "My standard" &&
                q.ApprenticeshipType == ApprenticeshipType.StandardCode &&
                q.ContactEmail == "guy@provider.com" &&
                q.ContactTelephone == "01234 5678902" &&
                q.ContactWebsite == "http://provider.com" &&
                q.CreatedByUser.UserId == User.UserId &&
                q.CreatedDate == Clock.UtcNow &&
                q.MarketingInformation == "My apprenticeship" &&
                q.ProviderId == provider.ProviderId &&
                q.StandardOrFramework.Standard.StandardCode == standardCode &&
                q.StandardOrFramework.Standard.Version == standardVersion &&
                q.Url == "http://provider.com/apprenticeship" &&
                q.Status == 2)));

            SqlQuerySpy.VerifyQuery<CreateApprenticeshipQASubmission, int>(q =>
                q.Apprenticeships.Single().ApprenticeshipId == apprenticeshipId &&
                q.Apprenticeships.Single().ApprenticeshipMarketingInformation == "My apprenticeship" &&
                q.Apprenticeships.Single().ApprenticeshipTitle == "My standard" &&
                q.ProviderId == provider.ProviderId &&
                q.ProviderMarketingInformation == "Provider 1 rocks" &&
                q.SubmittedByUserId == User.UserId &&
                q.SubmittedOn == Clock.UtcNow);

            SqlQuerySpy.VerifyQuery<SetProviderApprenticeshipQAStatus, None>(q =>
                q.ProviderId == provider.ProviderId && q.ApprenticeshipQAStatus == ApprenticeshipQAStatus.Submitted);

            var doc = await response.GetDocument();

            Assert.Equal(
                "Quality assurance submitted",
                doc.GetElementsByClassName("govuk-panel__title").Single().TextContent.Trim());
        }

        [Fact]
        public async Task PostConfirmation_ValidRequestWithNationalLocations_CreatesValidApprenticeship()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var standardCode = 123;
            var standardVersion = 1;
            var standard = await TestData.CreateStandard(standardCode, standardVersion, standardName: "My standard");

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetProviderDetails("Provider 1 rocks");
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            flowModel.SetApprenticeshipDetails(
                marketingInformation: "My apprenticeship",
                website: "http://provider.com/apprenticeship",
                contactTelephone: "01234 5678902",
                contactEmail: "guy@provider.com",
                contactWebsite: "http://provider.com");
            flowModel.SetApprenticeshipLocationType(ApprenticeshipLocationType.EmployerBased);
            flowModel.SetApprenticeshipIsNational(true);
            var mptxInstance = CreateMptxInstance(flowModel);

            Guid apprenticeshipId = default;
            CosmosDbQueryDispatcher.Callback<CreateApprenticeship, Success>(q => apprenticeshipId = q.Id);

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            CosmosDbQueryDispatcher.Verify(mock => mock.ExecuteQuery(It.Is<CreateApprenticeship>(q =>
                q.ApprenticeshipLocations.Single().ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased &&
                q.ApprenticeshipLocations.Single().National == true &&
                q.ApprenticeshipLocations.Single().VenueId == null)));
        }

        [Fact]
        public async Task PostConfirmation_ValidRequestWithRegions_CreatesValidApprenticeship()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var standardCode = 123;
            var standardVersion = 1;
            var standard = await TestData.CreateStandard(standardCode, standardVersion, standardName: "My standard");

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetProviderDetails("Provider 1 rocks");
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            flowModel.SetApprenticeshipDetails(
                marketingInformation: "My apprenticeship",
                website: "http://provider.com/apprenticeship",
                contactTelephone: "01234 5678902",
                contactEmail: "guy@provider.com",
                contactWebsite: "http://provider.com");
            flowModel.SetApprenticeshipLocationType(ApprenticeshipLocationType.EmployerBased);
            flowModel.SetApprenticeshipLocationRegionIds(new[]
            {
                "E06000001",  // County Durham
                "E10000009" // Dorset
            });
            var mptxInstance = CreateMptxInstance(flowModel);

            Guid apprenticeshipId = default;
            CosmosDbQueryDispatcher.Callback<CreateApprenticeship, Success>(q => apprenticeshipId = q.Id);

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            CosmosDbQueryDispatcher.Verify(mock => mock.ExecuteQuery(It.Is<CreateApprenticeship>(q =>
                q.ApprenticeshipLocations.Single().ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased &&
                q.ApprenticeshipLocations.Single().DeliveryModes.Single() == ApprenticeshipDeliveryMode.EmployerAddress &&
                q.ApprenticeshipLocations.Single().National == false &&
                q.ApprenticeshipLocations.Single().VenueId == null &&
                q.ApprenticeshipLocations.Single().Regions.Contains("E06000001") &&
                q.ApprenticeshipLocations.Single().Regions.Contains("E10000009"))));
        }

        [Fact]
        public async Task PostConfirmation_ValidRequestWithVenue_CreatesValidApprenticeship()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var standardCode = 123;
            var standardVersion = 1;
            var standard = await TestData.CreateStandard(standardCode, standardVersion, standardName: "My standard");

            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo())).VenueId;

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetProviderDetails("Provider 1 rocks");
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            flowModel.SetApprenticeshipDetails(
                marketingInformation: "My apprenticeship",
                website: "http://provider.com/apprenticeship",
                contactTelephone: "01234 5678902",
                contactEmail: "guy@provider.com",
                contactWebsite: "http://provider.com");
            flowModel.SetApprenticeshipLocationType(ApprenticeshipLocationType.ClassroomBased);
            flowModel.SetClassroomLocationForVenue(
                venueId,
                originalVenueId: null,
                radius: 5,
                deliveryModes: new[] { ApprenticeshipDeliveryMode.BlockRelease });
            var mptxInstance = CreateMptxInstance(flowModel);

            Guid apprenticeshipId = default;
            CosmosDbQueryDispatcher.Callback<CreateApprenticeship, Success>(q => apprenticeshipId = q.Id);

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            CosmosDbQueryDispatcher.Verify(mock => mock.ExecuteQuery(It.Is<CreateApprenticeship>(q =>
                q.ApprenticeshipLocations.Single().ApprenticeshipLocationType == ApprenticeshipLocationType.ClassroomBased &&
                q.ApprenticeshipLocations.Single().DeliveryModes.Single() == ApprenticeshipDeliveryMode.BlockRelease &&
                q.ApprenticeshipLocations.Single().Radius == 5 &&
                q.ApprenticeshipLocations.Single().VenueId == venueId)));
        }

        [Fact]
        public async Task PostConfirmation_ValidRequestWithRegionsAndVenue_CreatesValidApprenticeship()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var standardCode = 123;
            var standardVersion = 1;
            var standard = await TestData.CreateStandard(standardCode, standardVersion, standardName: "My standard");

            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo())).VenueId;

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetProviderDetails("Provider 1 rocks");
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            flowModel.SetApprenticeshipDetails(
                marketingInformation: "My apprenticeship",
                website: "http://provider.com/apprenticeship",
                contactTelephone: "01234 5678902",
                contactEmail: "guy@provider.com",
                contactWebsite: "http://provider.com");
            flowModel.SetApprenticeshipLocationType(ApprenticeshipLocationType.ClassroomBasedAndEmployerBased);
            flowModel.SetApprenticeshipLocationRegionIds(new[]
            {
                "E06000001",  // County Durham
                "E10000009" // Dorset
            });
            flowModel.SetClassroomLocationForVenue(
                venueId,
                originalVenueId: null,
                radius: 5,
                deliveryModes: new[] { ApprenticeshipDeliveryMode.BlockRelease });
            var mptxInstance = CreateMptxInstance(flowModel);

            Guid apprenticeshipId = default;
            CosmosDbQueryDispatcher.Callback<CreateApprenticeship, Success>(q => apprenticeshipId = q.Id);

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            CosmosDbQueryDispatcher.Verify(mock => mock.ExecuteQuery(It.Is<CreateApprenticeship>(q =>
                q.ApprenticeshipLocations.Any(l =>
                    l.ApprenticeshipLocationType == ApprenticeshipLocationType.ClassroomBased &&
                    l.DeliveryModes.Single() == ApprenticeshipDeliveryMode.BlockRelease &&
                    l.Radius == 5 &&
                    l.VenueId == venueId) &&
                q.ApprenticeshipLocations.Any(l =>
                    l.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased &&
                    l.DeliveryModes.Single() == ApprenticeshipDeliveryMode.EmployerAddress &&
                    l.National == false &&
                    l.VenueId == null &&
                    l.Regions.Contains("E06000001") &&
                    l.Regions.Contains("E10000009")))));
        }

        [Fact]
        public async Task PostConfirmation_ValidRequestWithExistingApprenticeshipRegionsAndVenue_UpdatesApprenticeship()
        {
            // Arrange
            var ukprn = 12347;
            var providerUserId = $"{ukprn}-user";
            var adminUserId = $"admin-user";
            var contactTelephone = "1111 111 1111";
            var contactWebsite = "https://somerandomprovider.com";
            var marketingInfo = "Providing Online training";
            var regions = new List<string> { "123" };
            var standardCode = 123;
            var standardVersion = 1;
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var adminUser = await TestData.CreateUser();
            var standard = await TestData.CreateStandard(standardCode, standardVersion, standardName: "My standard");
            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId,
                standard,
                createdBy: user,
                contactEmail: adminUser.Email,
                contactTelephone: contactTelephone,
                contactWebsite: contactWebsite,
                marketingInformation: marketingInfo,
                locations: new[]
                {
                    CreateApprenticeshipLocation.CreateRegions(regions)
                })).Id;
            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo())).VenueId;

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetProviderDetails("Provider 1 rocks");
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            flowModel.SetApprenticeshipDetails(
                marketingInformation: "My apprenticeship",
                website: "http://provider.com/apprenticeship",
                contactTelephone: "01234 5678902",
                contactEmail: "guy@provider.com",
                contactWebsite: "http://provider.com");
            flowModel.SetApprenticeshipLocationType(ApprenticeshipLocationType.ClassroomBasedAndEmployerBased);
            flowModel.SetApprenticeshipLocationRegionIds(new[]
            {
                "E06000001",  // County Durham
                "E10000009" // Dorset
            });
            flowModel.SetApprenticeshipId(apprenticeshipId);
            flowModel.SetClassroomLocationForVenue(
                venueId,
                originalVenueId: null,
                radius: 5,
                deliveryModes: new[] { ApprenticeshipDeliveryMode.BlockRelease });
            var mptxInstance = CreateMptxInstance(flowModel);

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            CosmosDbQueryDispatcher.Verify(mock => mock.ExecuteQuery(It.Is<UpdateApprenticeship>(q =>
                q.ApprenticeshipLocations.Any(l =>
                    l.ApprenticeshipLocationType == ApprenticeshipLocationType.ClassroomBased &&
                    l.DeliveryModes.Single() == ApprenticeshipDeliveryMode.BlockRelease &&
                    l.Radius == 5 &&
                    l.VenueId == venueId) &&
                q.ApprenticeshipLocations.Any(l =>
                    l.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased &&
                    l.DeliveryModes.Single() == ApprenticeshipDeliveryMode.EmployerAddress &&
                    l.National == false &&
                    l.VenueId == null &&
                    l.Regions.Contains("E06000001") &&
                    l.Regions.Contains("E10000009")))));
        }
    }
}
