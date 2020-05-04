using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Features.Apprenticeships.FindStandardOrFramework;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Apprenticeships
{
    public class FindStandardOrFrameworkTests : MvcTestBase
    {
        public FindStandardOrFrameworkTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_ProviderIsFEOnlyReturnsBadRequest()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.FE);

            var parentMptxInstance = MptxManager.CreateInstance(new ParentFlow());
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                 parentMptxInstance.InstanceId,
                 new FlowModel() { ProviderId = providerId },
                 new Dictionary<string, object>()
                 {
                    { "ReturnUrl", "callback" }
                 });

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeships/find-standard?ffiid={childMptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetSearch_NotEnoughCharactersReturnsError()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.Both);

            var parentMptxInstance = MptxManager.CreateInstance(new ParentFlow());
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                 parentMptxInstance.InstanceId,
                 new FlowModel() { ProviderId = providerId },
                 new Dictionary<string, object>()
                 {
                    { "ReturnUrl", "callback" }
                 });

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeships/find-standard/search?ffiid={childMptxInstance.InstanceId}&Search=h");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError(
                "Search",
                "Name or keyword for the apprenticeship this training is for must be 3 characters or more");
        }

        [Fact]
        public async Task GetSearch_RendersSearchResults()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.Both);

            await TestData.CreateStandard(standardCode: 123, version: 1, standardName: "Hairdressing");
            await TestData.CreateStandard(standardCode: 456, version: 2, standardName: "Hair");
            await TestData.CreateFramework(frameworkCode: 789, progType: 2, pathwayCode: 3, nasTitle: "Haircuts");

            var parentMptxInstance = MptxManager.CreateInstance(new ParentFlow());
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                 parentMptxInstance.InstanceId,
                 new FlowModel() { ProviderId = providerId },
                 new Dictionary<string, object>()
                 {
                    { "ReturnUrl", "callback" }
                 });

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeships/find-standard/search?ffiid={childMptxInstance.InstanceId}&Search=hair");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();

            Assert.Equal(
                "Found 3 results for hair",
                doc.GetElementById("pttcd-apprenticeships__find-provision__results-count").TextContent.Trim());
        }

        [Fact]
        public async Task GetSearch_RendersSearchResultsWithFrameworkWarning()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.Both);

            await TestData.CreateStandard(standardCode: 123, version: 1, standardName: "Hairdressing");
            await TestData.CreateStandard(standardCode: 456, version: 2, standardName: "Hair");
            await TestData.CreateFramework(frameworkCode: 789, progType: 2, pathwayCode: 3, nasTitle: "Haircuts");

            var parentMptxInstance = MptxManager.CreateInstance(new ParentFlow());
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                 parentMptxInstance.InstanceId,
                 new FlowModel() { ProviderId = providerId },
                 new Dictionary<string, object>()
                 {
                    { "ReturnUrl", "callback" }
                 });

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeships/find-standard/search?ffiid={childMptxInstance.InstanceId}&Search=hair");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();

            Assert.NotNull(doc.GetElementsByClassName("govuk-warning-text govuk-!-margin-bottom-2 govuk-!-margin-top-4"));
        }

        [Fact]
        public async Task GetSearch_RendersSearchResultsWithoutFrameworkWarning()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.Both);

            await TestData.CreateStandard(standardCode: 123, version: 1, standardName: "Hairdressing");
            await TestData.CreateStandard(standardCode: 456, version: 2, standardName: "Hair");

            var parentMptxInstance = MptxManager.CreateInstance(new ParentFlow());
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                 parentMptxInstance.InstanceId,
                 new FlowModel() { ProviderId = providerId },
                 new Dictionary<string, object>()
                 {
                    { "ReturnUrl", "callback" }
                 });

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeships/find-standard/search?ffiid={childMptxInstance.InstanceId}&Search=hair");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            var results = doc.GetElementsByClassName("govuk-warning-text govuk-!-margin-bottom-2 govuk-!-margin-top-4");

            Assert.Empty(results);
        }

        [Fact]
        public async Task GetSelect_ValidRequest_UpdatesParentStateAndRedirects()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.Both);

            await TestData.CreateStandard(standardCode: 123, version: 1, standardName: "Hairdressing");
            await TestData.CreateStandard(standardCode: 456, version: 2, standardName: "Hair");
            await TestData.CreateFramework(frameworkCode: 789, progType: 2, pathwayCode: 3, nasTitle: "Haircuts");

            var parentMptxInstance = MptxManager.CreateInstance(new ParentFlow());
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                 parentMptxInstance.InstanceId,
                 new FlowModel() { ProviderId = providerId },
                 new Dictionary<string, object>()
                 {
                    { "ReturnUrl", "callback" }
                 });

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeships/find-standard/select?ffiid={childMptxInstance.InstanceId}&&standardOrFrameworkType=standard&standardCode=123&version=1");

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("callback", response.Headers.Location.OriginalString);

            Assert.True(parentMptxInstance.State.StandardOrFramework.IsStandard);
            Assert.Equal(123, parentMptxInstance.State.StandardOrFramework.Standard.StandardCode);
            Assert.Equal(1, parentMptxInstance.State.StandardOrFramework.Standard.Version);
            Assert.Equal("Hairdressing", parentMptxInstance.State.StandardOrFramework.Standard.StandardName);
        }

        private class ParentFlow : IFlowModelCallback
        {
            public StandardOrFramework StandardOrFramework { get; set; }

            public void ReceiveStandardOrFramework(StandardOrFramework standardOrFramework) =>
                StandardOrFramework = standardOrFramework;
        }
    }
}
