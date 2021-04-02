using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Moq;
using Xunit;
using FlowModel = Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider.FlowModel;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.NewApprenticeshipProvider
{
    public class ProviderDetailTests : MvcTestBase
    {
        public ProviderDetailTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_HelpdeskUserCannotAccess()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsHelpdesk();

            var mptxInstance = CreateMptxInstance(new FlowModel());

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/provider-detail?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}");

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

            var mptxInstance = CreateMptxInstance(new FlowModel());

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/provider-detail?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}");

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

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var mptxInstance = CreateMptxInstance(new FlowModel());

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/provider-detail?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Get_RendersExpectedOutput()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider name",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted,
                marketingInformation: "<p>Existing marketing info</p>");

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetProviderDetails("<p>Existing marketing info</p>");
            var mptxInstance = CreateMptxInstance(flowModel);

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/provider-detail?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("<p>Existing marketing info</p>", doc.GetElementById("MarketingInformation").TextContent.Trim());
        }

        [Fact]
        public async Task Post_HelpdeskUserCannotAccess()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider name",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted,
                courseDirectoryName: "Alias");

            await User.AsHelpdesk();

            var mptxInstance = CreateMptxInstance(new FlowModel());

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("MarketingInformation", "<p>New marketing info</p>")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/provider-detail?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
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
            var provider = await TestData.CreateProvider(
                providerName: "Provider name",
                apprenticeshipQAStatus: qaStatus,
                courseDirectoryName: "Alias");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            var mptxInstance = CreateMptxInstance(new FlowModel());

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("MarketingInformation", "<p>New marketing info</p>")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/provider-detail?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_NotApprenticeshipProviderReturnsForbidden()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerType: ProviderType.FE,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            var mptxInstance = CreateMptxInstance(new FlowModel());

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("MarketingInformation", "<p>New marketing info</p>")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/provider-detail?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Post_InvalidMarketingInfoRendersErrorMessage()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider name",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted,
                courseDirectoryName: "Alias");

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var mptxInstance = CreateMptxInstance(new FlowModel());

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("MarketingInformation", new string('x', 751))
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/provider-detail?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("MarketingInformation", "Brief overview of your organisation for employers must be 750 characters or fewer");
        }

        [Fact]
        public async Task Post_ValidRequestUpdatesStateAndRedirects()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider name",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted,
                courseDirectoryName: "Alias");

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var mptxInstance = CreateMptxInstance(new FlowModel());

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("MarketingInformation", "<p>New marketing info</p>")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/provider-detail?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            var state = GetMptxInstance<FlowModel>(mptxInstance.InstanceId).State;
            Assert.True(state.GotProviderDetails);
            Assert.Equal("<p>New marketing info</p>", state.ProviderMarketingInformation);

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.StartsWith(
                "/new-apprenticeship-provider/provider-detail-confirmation",
                response.Headers.Location.OriginalString);
        }

        [Fact]
        public async Task GetConfirmation_HelpdeskUserCannotAccess()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted,
                providerName: "Test Provider",
                courseDirectoryName: "CD Name");

            await User.AsHelpdesk();

            var flowModel = new FlowModel();
            flowModel.SetProviderDetails("<p>New marketing info</p>");
            var mptxInstance = CreateMptxInstance(flowModel);

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/provider-detail-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData(ApprenticeshipQAStatus.Submitted)]
        [InlineData(ApprenticeshipQAStatus.InProgress)]
        [InlineData(ApprenticeshipQAStatus.Passed)]
        [InlineData(ApprenticeshipQAStatus.UnableToComplete)]
        public async Task GetConfirmation_QAStatusNotValidReturnsBadRequest(ApprenticeshipQAStatus qaStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: qaStatus);

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            var flowModel = new FlowModel();
            flowModel.SetProviderDetails("<p>New marketing info</p>");
            var mptxInstance = CreateMptxInstance(flowModel);

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/provider-detail-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetConfirmation_NotApprenticeshipProviderReturnsForbidden()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted,
                providerType: ProviderType.FE);

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            var flowModel = new FlowModel();
            flowModel.SetProviderDetails("<p>New marketing info</p>");
            var mptxInstance = CreateMptxInstance(flowModel);

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/provider-detail-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetConfirmation_NoProviderDetailSetReturnsBadRequest()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var mptxInstance = CreateMptxInstance(new FlowModel());

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/provider-detail-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetConfirmation_RendersExpectedOutput()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted,
                providerName: "Test Provider",
                courseDirectoryName: "CD Name");

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetProviderDetails("<p>New marketing info</p>");
            var mptxInstance = CreateMptxInstance(flowModel);

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/provider-detail-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task PostConfirmation_HelpdeskUserCannotAccess()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsHelpdesk();

            var flowModel = new FlowModel();
            flowModel.SetProviderDetails("<p>New marketing info</p>");
            var mptxInstance = CreateMptxInstance(flowModel);

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/provider-detail-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData(ApprenticeshipQAStatus.Submitted)]
        [InlineData(ApprenticeshipQAStatus.InProgress)]
        [InlineData(ApprenticeshipQAStatus.Passed)]
        [InlineData(ApprenticeshipQAStatus.UnableToComplete)]
        public async Task PostConfirmation_QAStatusNotValidReturnsBadRequest(ApprenticeshipQAStatus qaStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: qaStatus);

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetProviderDetails("<p>New marketing info</p>");
            var mptxInstance = CreateMptxInstance(flowModel);

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/provider-detail-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostConfirmation_NotApprenticeshipProviderReturnsForbidden()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted,
                providerType: ProviderType.FE);

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            var flowModel = new FlowModel();
            flowModel.SetProviderDetails("<p>New marketing info</p>");
            var mptxInstance = CreateMptxInstance(flowModel);

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/provider-detail-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task PostConfirmation_NoProviderDetailSetReturnsBadRequest()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var mptxInstance = CreateMptxInstance(new FlowModel());

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/provider-detail-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostConfirmation_ValidRequestUpdatesDbAndRedirects()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetProviderDetails("<p>New marketing info</p>");
            var mptxInstance = CreateMptxInstance(flowModel);

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/provider-detail-confirmation?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            CosmosDbQueryDispatcher.Verify(mock => mock.ExecuteQuery(It.Is<UpdateProviderInfo>(c =>
                c.ProviderId == provider.ProviderId &&
                c.MarketingInformation == "<p>New marketing info</p>" &&
                c.UpdatedBy.UserId == User.UserId &&
                c.UpdatedOn == Clock.UtcNow)));

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(
                "/new-apprenticeship-provider/find-standard",
                UrlHelper.StripQueryParams(response.Headers.Location.OriginalString));
        }
    }
}
