using System;
using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.ApprenticeshipQA
{
    public class ListProvidersTest : MvcTestBase
    {
        public ListProvidersTest(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Get_ProviderUserCannotAccess(TestUserType userType)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            await User.AsTestUser(userType, provider.ProviderId);

            // Act
            var response = await HttpClient.GetAsync("apprenticeship-qa");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Get_RendersRowsCorrectly()
        {
            // Arrange

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var provider1 = await TestData.CreateProvider(
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);
            var provider1User = await TestData.CreateUser(providerId: provider1.ProviderId);
            await TestData.CreateUserSignIn(provider1User.UserId, new DateTime(2018, 4, 1, 10, 4, 3));
            var provider1ApprenticeshipId = (await TestData.CreateApprenticeship(provider1.ProviderId, standard, createdBy: User.ToUserInfo())).ApprenticeshipId;
            await TestData.CreateApprenticeshipQASubmission(
                provider1.ProviderId,
                submittedOn: new DateTime(2018, 4, 1, 12, 30, 37),
                submittedByUserId: provider1User.UserId,
                providerMarketingInformation: "Provider 1 overview",
                apprenticeshipIds: new[] { provider1ApprenticeshipId });

            var provider2 = await TestData.CreateProvider(
                providerName: "Provider 2",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);
            var provider2User = await TestData.CreateUser(providerId: provider2.ProviderId);
            await TestData.CreateUserSignIn(provider2User.UserId, new DateTime(2019, 5, 3, 14, 55, 17));
            var provider2ApprenticeshipId = (await TestData.CreateApprenticeship(provider2.ProviderId, standard, createdBy: User.ToUserInfo())).ApprenticeshipId;
            await TestData.CreateApprenticeshipQASubmission(
                provider2.ProviderId,
                submittedOn: new DateTime(2019, 5, 3, 15, 01, 23),
                submittedByUserId: provider2User.UserId,
                providerMarketingInformation: "Provider 2 overview",
                apprenticeshipIds: new[] { provider2ApprenticeshipId });

            var provider3 = await TestData.CreateProvider(
                providerName: "Provider 3",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);
            var provider3User = await TestData.CreateUser(providerId: provider3.ProviderId);
            await TestData.CreateUserSignIn(provider3User.UserId, new DateTime(2019, 2, 6, 7, 22, 9));

            // TODO Add more here once we have a way of modelling other statuses

            // Act
            var response = await HttpClient.GetAsync("apprenticeship-qa");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();

            var newProviders = doc.QuerySelector("#notstarted");
            var newProviderRows = newProviders.QuerySelectorAll("tbody>tr");

            var firstNewProviderRow = newProviderRows[0];
            Assert.Equal("Provider 3", firstNewProviderRow.QuerySelector(":nth-child(1)").TextContent);
            Assert.Equal(provider3.Ukprn.ToString(), firstNewProviderRow.QuerySelector(":nth-child(2)").TextContent);
            Assert.Equal("06 Feb 2019", firstNewProviderRow.QuerySelector(":nth-child(3)").TextContent);

            var submitted = doc.QuerySelector("#submitted");
            var submittedRows = submitted.QuerySelectorAll("tbody>tr");

            var firstSubmittedRow = submittedRows[0];
            Assert.Equal("Provider 2", firstSubmittedRow.QuerySelector(":nth-child(1)").TextContent);
            Assert.Equal(provider2.Ukprn.ToString(), firstSubmittedRow.QuerySelector(":nth-child(2)").TextContent);
            Assert.Equal("03 May 2019", firstSubmittedRow.QuerySelector(":nth-child(3)").TextContent);

            var secondSubmittedRow = submittedRows[1];
            Assert.Equal("Provider 1", secondSubmittedRow.QuerySelector(":nth-child(1)").TextContent);
            Assert.Equal(provider1.Ukprn.ToString(), secondSubmittedRow.QuerySelector(":nth-child(2)").TextContent);
            Assert.Equal("01 Apr 2018", secondSubmittedRow.QuerySelector(":nth-child(3)").TextContent);
        }

        [Fact]
        public async Task Get_NoProvidersRendersEmptyStateMessage()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("apprenticeship-qa");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();

            Assert.Equal("You have no new providers.", doc.QuerySelector("#notstarted p").TextContent);
            Assert.Equal("You have no providers ready to QA.", doc.QuerySelector("#submitted p").TextContent);
            Assert.Equal("You have no providers in progress.", doc.QuerySelector("#in-progress p").TextContent);
            Assert.Equal("You have no providers that have failed QA.", doc.QuerySelector("#fail p").TextContent);
            Assert.Equal("You have no providers unable to complete.", doc.QuerySelector("#unable-to-complete p").TextContent);
        }
    }
}
