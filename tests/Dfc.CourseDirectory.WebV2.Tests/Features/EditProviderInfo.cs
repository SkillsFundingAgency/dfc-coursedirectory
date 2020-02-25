using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Dfc.CourseDirectory.WebV2.Models;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.Features
{
    public class EditProviderInfo : TestBase
    {
        public EditProviderInfo(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_RendersExpectedOutput()
        {
            // Arrange
            var ukprn = 12345;

            await TestData.CreateProvider(
                ukprn,
                providerType: ProviderType.Apprenticeships,
                alias: "Current alias",
                courseDirectoryName: "Current CD name",
                marketingInformation: "Current overview");

            var url = $"/providers/info?ukprn={ukprn}";

            // Act
            var response = await HttpClient.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode();
            var doc = await response.GetDocument();
            Assert.Equal("Current alias", doc.GetElementById("Alias").As<IHtmlInputElement>().Value);
            Assert.Equal("Current CD name", doc.GetElementById("CourseDirectoryName").As<IHtmlInputElement>().Value);
            Assert.Equal("Current overview", doc.GetElementById("BriefOverview").As<IHtmlTextAreaElement>().InnerHtml);
        }

        [Theory]
        [InlineData("", "Provider name", "Provider name")]
        [InlineData(null, "Provider name", "Provider name")]
        [InlineData("CD name", "Provider name", "CD name")]
        public async Task Get_CourseDirectoryName_DerivedCorrectly(
            string courseDirectoryName,
            string providerName,
            string expectedValue)
        {
            // Arrange
            var ukprn = 12345;

            await TestData.CreateProvider(
                ukprn,
                providerType: ProviderType.Apprenticeships,
                providerName: providerName,
                courseDirectoryName: courseDirectoryName);

            var url = $"/providers/info?ukprn={ukprn}";

            // Act
            var response = await HttpClient.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode();
            var doc = await response.GetDocument();
            Assert.Equal(expectedValue, doc.GetElementById("CourseDirectoryName").As<IHtmlInputElement>().Value);
        }

        [Theory]
        [InlineData(TestUserType.Helpdesk)]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Get_UserNotDeveloper_DoesNotRenderCourseDirectoryName(TestUserType userType)
        {
            // Arrange
            var ukprn = 12345;

            await TestData.CreateProvider(
                ukprn,
                providerType: ProviderType.Apprenticeships,
                alias: "Current alias",
                courseDirectoryName: "Current CD name",
                marketingInformation: "Current overview");

            User.AsTestUser(userType, ukprn);

            var url = $"/providers/info?ukprn={ukprn}";

            // Act
            var response = await HttpClient.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode();
            var doc = await response.GetDocument();
            Assert.Null(doc.GetElementById("CourseDirectoryName"));
        }

        [Fact]
        public async Task Get_ProviderTypeIsFE_DoesNotRenderBriefOverview()
        {
            // Arrange
            var ukprn = 12345;

            await TestData.CreateProvider(
                ukprn,
                providerType: ProviderType.FE);

            var url = $"/providers/info?ukprn={ukprn}";

            // Act
            var response = await HttpClient.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode();
            var doc = await response.GetDocument();

            Assert.Null(doc.GetElementById("BriefOverview"));
        }

        [Fact]
        public async Task Get_UserIsDeveloper_RendersEditableBriefOverview()
        {
            // Arrange
            var ukprn = 12345;

            await TestData.CreateProvider(
                ukprn,
                providerType: ProviderType.Apprenticeships);

            User.AsDeveloper();

            var url = $"/providers/info?ukprn={ukprn}";

            // Act
            var response = await HttpClient.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode();
            var doc = await response.GetDocument();

            throw new NotImplementedException();
        }

        [Fact]
        public async Task Get_UserIsHelpdesk_RendersReadOnlyBriefOverview()
        {
            // Arrange
            var ukprn = 12345;

            await TestData.CreateProvider(
                ukprn,
                providerType: ProviderType.Apprenticeships);

            User.AsHelpdesk();

            var url = $"/providers/info?ukprn={ukprn}";

            // Act
            var response = await HttpClient.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode();
            var doc = await response.GetDocument();

            throw new NotImplementedException();
        }

        [Theory]
        [InlineData(ApprenticeshipQAStatus.Passed)]
        [InlineData(ApprenticeshipQAStatus.Submitted)]
        public async Task Get_UserIsProvider_RendersReadOnlyBriefOverviewWhenQACompletedOrSubmitted(ApprenticeshipQAStatus qaStatus)
        {
            // Arrange
            var ukprn = 12345;

            await TestData.CreateProvider(
                ukprn,
                providerType: ProviderType.Apprenticeships,
                apprenticeshipQAStatus: qaStatus);

            User.AsHelpdesk();

            var url = $"/providers/info?ukprn={ukprn}";

            // Act
            var response = await HttpClient.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode();
            var doc = await response.GetDocument();

            throw new NotImplementedException();
        }

        [Theory]
        [InlineData(ApprenticeshipQAStatus.Failed)]
        [InlineData(ApprenticeshipQAStatus.InProgress)]
        [InlineData(ApprenticeshipQAStatus.NotStarted)]
        public async Task Get_UserIsProvider_RendersEditableBriefOverviewWhenQANotCompleted(ApprenticeshipQAStatus qaStatus)
        {
            // Arrange
            var ukprn = 12345;

            await TestData.CreateProvider(
                ukprn,
                providerType: ProviderType.Apprenticeships,
                apprenticeshipQAStatus: qaStatus);

            User.AsHelpdesk();

            var url = $"/providers/info?ukprn={ukprn}";

            // Act
            var response = await HttpClient.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode();
            var doc = await response.GetDocument();

            throw new NotImplementedException();
        }

        [Fact]
        public async Task Get_PersistedDataHasValidationErrors_ShowsErrors()
        {
            // Arrange
            var ukprn = 12345;

            var longerThan100CharactersString = new string('z', 101);

            await TestData.CreateProvider(
                ukprn,
                providerType: ProviderType.Apprenticeships,
                alias: longerThan100CharactersString,
                courseDirectoryName: longerThan100CharactersString,
                marketingInformation: "Current overview");  // FIXME

            var url = $"/providers/info?ukprn={ukprn}";

            // Act
            var response = await HttpClient.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode();
            var doc = await response.GetDocument();
            Assert.NotNull(doc.GetElementById("Alias-error"));
            Assert.NotNull(doc.GetElementById("CourseDirectoryName-error"));
            Assert.NotNull(doc.GetElementById("BriefOverview-error"));
        }

        [Fact]
        public async Task Post_ValidRequest_SubmitsSuccessfullyAndRedirects()
        {
            // Arrange
            var ukprn = 12345;

            await TestData.CreateProvider(ukprn, providerType: ProviderType.Apprenticeships);

            var url = $"/providers/info?ukprn={ukprn}";

            var content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "Alias", "My alias" },
                { "CourseDirectoryName", "CD Name" },
                { "BriefOverview", "Overview" }
            });

            // Act
            var response = await HttpClient.PostAsync(url, content);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("/provider/details?ukprn=12345", response.Headers.Location.OriginalString);
        }

        [Fact]
        public async Task Post_AliasValidationError_ReturnsBadRequest()
        {
            // Arrange
            var ukprn = 12345;

            await TestData.CreateProvider(ukprn, providerType: ProviderType.Apprenticeships);

            var url = $"/providers/info?ukprn={ukprn}";

            var content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "Alias", new string('z', 101) },
                { "CourseDirectoryName", "CD Name" },
                { "BriefOverview", "Overview" }
            });

            // Act
            var response = await HttpClient.PostAsync(url, content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var doc = await response.GetDocument();
            Assert.NotNull(doc.GetElementById("Alias-error"));
        }

        [Fact]
        public async Task Post_CourseDirectoryNameValidationError_ReturnsBadRequest()
        {
            // Arrange
            var ukprn = 12345;

            await TestData.CreateProvider(ukprn, providerType: ProviderType.Apprenticeships);

            var url = $"/providers/info?ukprn={ukprn}";

            var content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "Alias", "My alias" },
                { "CourseDirectoryName", new string('z', 101) },
                { "BriefOverview", "Overview" }
            });

            // Act
            var response = await HttpClient.PostAsync(url, content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var doc = await response.GetDocument();
            Assert.NotNull(doc.GetElementById("CourseDirectoryName-error"));
        }

        [Fact]
        public async Task Post_BriefOverviewValidationError_ReturnsBadRequest()
        {
            // Arrange
            var ukprn = 12345;

            await TestData.CreateProvider(ukprn, providerType: ProviderType.Apprenticeships);

            var url = $"/providers/info?ukprn={ukprn}";

            var content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "Alias", "My alias" },
                { "CourseDirectoryName", "CD Name" },
                { "BriefOverview", "Overview" }  // FIXME
            });

            // Act
            var response = await HttpClient.PostAsync(url, content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var doc = await response.GetDocument();
            Assert.NotNull(doc.GetElementById("BriefOverview-error"));
        }
    }
}
