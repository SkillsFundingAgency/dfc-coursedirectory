using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider;
using Dfc.CourseDirectory.WebV2.Models;
using Moq;
using OneOf.Types;
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
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

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
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={providerId}&ffiid={mptxInstance.InstanceId}");

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
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: qaStatus);

            var standardCode = 123;
            var standardVersion = 1;
            var standard = await TestData.CreateStandard(standardCode, standardVersion, standardName: "My standard");

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

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
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={providerId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Get_NotApprenticeshipProvider_ReturnsBadRequest()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted,
                providerType: ProviderType.FE);

            var standardCode = 123;
            var standardVersion = 1;
            var standard = await TestData.CreateStandard(standardCode, standardVersion, standardName: "My standard");

            await User.AsProviderUser(providerId, ProviderType.FE);

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
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={providerId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Get_IncompleteFlow_ReturnsBadRequest()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            var mptxInstance = CreateMptxInstance(flowModel);

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={providerId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Get_ValidRequest_RendersExpectedOutput()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var standardCode = 123;
            var standardVersion = 1;
            var standard = await TestData.CreateStandard(standardCode, standardVersion, standardName: "My standard");

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

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
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={providerId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("My apprenticeship", doc.GetSummaryListValueWithKey("Apprenticeship information for employers"));
            Assert.Equal("http://provider.com/apprenticeship", doc.GetSummaryListValueWithKey("Apprenticeship website"));
            Assert.Equal("guy@provider.com", doc.GetSummaryListValueWithKey("Email"));
            Assert.Equal("01234 5678902", doc.GetSummaryListValueWithKey("Telephone"));
            Assert.Equal("http://provider.com", doc.GetSummaryListValueWithKey("Contact URL"));
        }

        [Fact]
        public async Task Post_HelpdeskUser_ReturnsForbidden()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

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
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={providerId}&ffiid={mptxInstance.InstanceId}",
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
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: qaStatus);

            var standardCode = 123;
            var standardVersion = 1;
            var standard = await TestData.CreateStandard(standardCode, standardVersion, standardName: "My standard");

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

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
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={providerId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_NotApprenticeshipProvider_ReturnsBadRequest()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted,
                providerType: ProviderType.FE);

            var standardCode = 123;
            var standardVersion = 1;
            var standard = await TestData.CreateStandard(standardCode, standardVersion, standardName: "My standard");

            await User.AsProviderUser(providerId, ProviderType.FE);

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
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={providerId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_IncompleteFlow_ReturnsBadRequest()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            var mptxInstance = CreateMptxInstance(flowModel);

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={providerId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostConfirmation_ValidRequestWithNationalLocations_CreatesApprenticeshipQASubmissionUpdatesQAStatusAndReturnsConfirmation()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var standardCode = 123;
            var standardVersion = 1;
            var standard = await TestData.CreateStandard(standardCode, standardVersion, standardName: "My standard");

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

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
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={providerId}&ffiid={mptxInstance.InstanceId}",
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
                q.ProviderId == providerId &&
                q.StandardOrFramework.Standard.StandardCode == standardCode &&
                q.StandardOrFramework.Standard.Version == standardVersion &&
                q.Url == "http://provider.com/apprenticeship")));

            SqlQuerySpy.VerifyQuery<CreateApprenticeshipQASubmission, int>(q =>
                q.Apprenticeships.Single().ApprenticeshipId == apprenticeshipId &&
                q.Apprenticeships.Single().ApprenticeshipMarketingInformation == "My apprenticeship" &&
                q.Apprenticeships.Single().ApprenticeshipTitle == "My standard" &&
                q.ProviderId == providerId &&
                q.ProviderMarketingInformation == "Provider 1 rocks" &&
                q.SubmittedByUserId == User.UserId &&
                q.SubmittedOn == Clock.UtcNow);

            SqlQuerySpy.VerifyQuery<SetProviderApprenticeshipQAStatus, None>(q =>
                q.ProviderId == providerId && q.ApprenticeshipQAStatus == ApprenticeshipQAStatus.Submitted);

            var doc = await response.GetDocument();

            Assert.Equal(
                "Information submitted",
                doc.GetElementsByClassName("govuk-panel__title").Single().TextContent.Trim());
        }

        [Fact]
        public async Task PostConfirmation_ValidRequestWithNationalLocations_CreatesValidApprenticeship()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var standardCode = 123;
            var standardVersion = 1;
            var standard = await TestData.CreateStandard(standardCode, standardVersion, standardName: "My standard");

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

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
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={providerId}&ffiid={mptxInstance.InstanceId}",
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
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var standardCode = 123;
            var standardVersion = 1;
            var standard = await TestData.CreateStandard(standardCode, standardVersion, standardName: "My standard");

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

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
                $"new-apprenticeship-provider/apprenticeship-confirmation?providerId={providerId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            CosmosDbQueryDispatcher.Verify(mock => mock.ExecuteQuery(It.Is<CreateApprenticeship>(q =>
                q.ApprenticeshipLocations.Single().ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased &&
                q.ApprenticeshipLocations.Single().National == false &&
                q.ApprenticeshipLocations.Single().VenueId == null &&
                q.ApprenticeshipLocations.Single().Regions.Contains("E06000001") &&
                q.ApprenticeshipLocations.Single().Regions.Contains("E10000009"))));
        }
    }
}
