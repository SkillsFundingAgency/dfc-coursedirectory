using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Xunit;
using FlowModel = Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider.FlowModel;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.NewApprenticeshipProvider
{
    public class ApprenticeshipDetailsTests : MvcTestBase
    {
        public ApprenticeshipDetailsTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_HelpdeskUserCannotAccess()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var standard = await TestData.CreateStandard(standardCode: 123, version: 1, standardName: "My standard");

            await User.AsHelpdesk();

            var flowModel = new FlowModel();
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            var mptxInstance = CreateMptxInstance(flowModel);

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-details?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}");

            // Act
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

            var standard = await TestData.CreateStandard(standardCode: 123, version: 1, standardName: "My standard");

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            var mptxInstance = CreateMptxInstance(flowModel);

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-details?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}");

            // Act
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Get_NotApprenticeshipProviderReturnsForbidden()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerType: ProviderType.FE,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var standard = await TestData.CreateStandard(standardCode: 123, version: 1, standardName: "My standard");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            var flowModel = new FlowModel();
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            var mptxInstance = CreateMptxInstance(flowModel);

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-details?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}");

            // Act
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Get_RendersExpectedOutput()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var standard = await TestData.CreateStandard(standardCode: 123, version: 1, standardName: "My standard");

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            var mptxInstance = CreateMptxInstance(flowModel);

            // Act
            var response = await HttpClient.GetAsync(
                $"new-apprenticeship-provider/apprenticeship-details?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}");

            // Act
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal(
                "My standard",
                doc.GetElementById("pttcd-new-apprenticeship-provider__apprenticeship-details__standard-name").TextContent);
        }

        [Fact]
        public async Task Post_HelpdeskUserCannotAccess()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var standard = await TestData.CreateStandard(standardCode: 123, version: 1, standardName: "My standard");

            await User.AsHelpdesk();

            var flowModel = new FlowModel();
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            var mptxInstance = CreateMptxInstance(flowModel);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("MarketingInformation", "Apprenticeship info")
                .Add("ContactEmail", "guy@provider1.com")
                .Add("ContactTelephone", "01234 567890")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-details?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Act
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

            var standard = await TestData.CreateStandard(standardCode: 123, version: 1, standardName: "My standard");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            var flowModel = new FlowModel();
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            var mptxInstance = CreateMptxInstance(flowModel);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("MarketingInformation", "Apprenticeship info")
                .Add("ContactEmail", "guy@provider1.com")
                .Add("ContactTelephone", "01234 567890")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-details?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Act
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_NotApprenticeshipProviderReturnsForbidden()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerType: ProviderType.FE,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var standard = await TestData.CreateStandard(standardCode: 123, version: 1, standardName: "My standard");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            var flowModel = new FlowModel();
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            var mptxInstance = CreateMptxInstance(flowModel);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("MarketingInformation", "Apprenticeship info")
                .Add("ContactEmail", "guy@provider1.com")
                .Add("ContactTelephone", "01234 567890")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-details?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Act
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData("")]
        public async Task Post_InvalidMarketingInformationRendersError(string marketingInfo)
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var standard = await TestData.CreateStandard(standardCode: 123, version: 1, standardName: "My standard");

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            var mptxInstance = CreateMptxInstance(flowModel);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("MarketingInformation", marketingInfo)
                .Add("ContactEmail", "guy@provider1.com")
                .Add("ContactTelephone", "01234 567890")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-details?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Act
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError(
                "MarketingInformation",
                "Enter apprenticeship information for employers");
        }

        [Theory]
        [InlineData("/foo")]
        public async Task Post_InvalidWebsiteRendersError(string website)
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var standard = await TestData.CreateStandard(standardCode: 123, version: 1, standardName: "My standard");

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            var mptxInstance = CreateMptxInstance(flowModel);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("MarketingInformation", "Apprenticeship info")
                .Add("ContactEmail", "guy@provider1.com")
                .Add("ContactTelephone", "01234 567890")
                .Add("Website", website)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-details?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Act
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError(
                "Website",
                "Website must be a real webpage, like https://www.provider.com/apprenticeship");
        }

        [Theory]
        [InlineData("", "Enter email")]
        [InlineData("guy", "Email must be a valid email address")]
        public async Task Post_InvalidContactEmailRendersError(
            string contactEmail,
            string expectedErrorMessage)
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var standard = await TestData.CreateStandard(standardCode: 123, version: 1, standardName: "My standard");

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            var mptxInstance = CreateMptxInstance(flowModel);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("MarketingInformation", "Apprenticeship info")
                .Add("ContactEmail", contactEmail)
                .Add("ContactTelephone", "01234 567890")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-details?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Act
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("ContactEmail", expectedErrorMessage);
        }

        [Theory]
        [InlineData("", "Enter telephone")]
        [InlineData("xx", "Telephone must be a valid UK phone number")]
        public async Task Post_InvalidContactTelephoneRendersError(
            string contactTelephone,
            string expectedErrorMessage)
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var standard = await TestData.CreateStandard(standardCode: 123, version: 1, standardName: "My standard");

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            var mptxInstance = CreateMptxInstance(flowModel);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("MarketingInformation", "Apprenticeship info")
                .Add("ContactEmail", "guy@provider1.com")
                .Add("ContactTelephone", contactTelephone)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-details?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Act
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("ContactTelephone", expectedErrorMessage);
        }

        [Theory]
        [InlineData("/foo")]
        public async Task Post_InvalidContactWebsiteRendersError(string contactWebsite)
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var standard = await TestData.CreateStandard(standardCode: 123, version: 1, standardName: "My standard");

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            var mptxInstance = CreateMptxInstance(flowModel);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("MarketingInformation", "Apprenticeship info")
                .Add("ContactEmail", "guy@provider1.com")
                .Add("ContactTelephone", "01234 567890")
                .Add("ContactWebsite", contactWebsite)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-details?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Act
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError(
                "ContactWebsite",
                "Contact us page must be a real webpage, like https://www.provider.com/apprenticeship");
        }

        [Fact]
        public async Task Post_ValidRequestRedirects()
        {
            // Arrange
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var standard = await TestData.CreateStandard(standardCode: 123, version: 1, standardName: "My standard");

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var flowModel = new FlowModel();
            flowModel.SetApprenticeshipStandardOrFramework(standard);
            var mptxInstance = CreateMptxInstance(flowModel);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("MarketingInformation", "Apprenticeship info")
                .Add("ContactEmail", "guy@provider1.com")
                .Add("ContactTelephone", "01234 567890")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/apprenticeship-details?providerId={provider.ProviderId}&ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Act
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.StartsWith(
                "/new-apprenticeship-provider/apprenticeship-locations",
                response.Headers.Location.OriginalString);
        }
    }
}
