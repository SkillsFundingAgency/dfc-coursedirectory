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
            var providerId = await TestData.CreateProvider();

            await User.AsTestUser(userType, providerId);

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

            var provider1Ukprn = 12345;
            var provider1UserId = $"user-{provider1Ukprn}";
            var provider1Id = await TestData.CreateProvider(
                ukprn: provider1Ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);
            await TestData.CreateUser(provider1UserId, "guy@provider1.com", "Provider 1", "User", provider1Id);
            await TestData.CreateUserSignIn(provider1UserId, new DateTime(2018, 4, 1, 10, 4, 3));
            var provider1ApprenticeshipId = await TestData.CreateApprenticeship(provider1Id, standard, createdBy: User.ToUserInfo());
            await TestData.CreateApprenticeshipQASubmission(
                provider1Id,
                submittedOn: new DateTime(2018, 4, 1, 12, 30, 37),
                submittedByUserId: provider1UserId,
                providerMarketingInformation: "Provider 1 overview",
                apprenticeshipIds: new[] { provider1ApprenticeshipId });

            var provider2Ukprn = 23456;
            var provider2UserId = $"user-{provider2Ukprn}";
            var provider2Id = await TestData.CreateProvider(
                ukprn: provider2Ukprn,
                providerName: "Provider 2",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);
            await TestData.CreateUser(provider2UserId, "guy@provider2.com", "Provider 2", "User", provider2Id);
            await TestData.CreateUserSignIn(provider2UserId, new DateTime(2019, 5, 3, 14, 55, 17));
            var provider2ApprenticeshipId = await TestData.CreateApprenticeship(provider2Id, standard, createdBy: User.ToUserInfo());
            await TestData.CreateApprenticeshipQASubmission(
                provider2Id,
                submittedOn: new DateTime(2019, 5, 3, 15, 01, 23),
                submittedByUserId: provider2UserId,
                providerMarketingInformation: "Provider 2 overview",
                apprenticeshipIds: new[] { provider2ApprenticeshipId });

            var provider3Ukprn = 345678;
            var provider3UserId = $"user-{provider3Ukprn}";
            var provider3Id = await TestData.CreateProvider(
                ukprn: provider3Ukprn,
                providerName: "Provider 3",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);
            await TestData.CreateUser(provider3UserId, "guy@provider3.com", "Provider 3", "User", provider3Id);
            await TestData.CreateUserSignIn(provider3UserId, new DateTime(2019, 2, 6, 7, 22, 9));

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
            Assert.Equal("345678", firstNewProviderRow.QuerySelector(":nth-child(2)").TextContent);
            Assert.Equal("06 Feb 2019", firstNewProviderRow.QuerySelector(":nth-child(3)").TextContent);

            var submitted = doc.QuerySelector("#submitted");
            var submittedRows = submitted.QuerySelectorAll("tbody>tr");

            var firstSubmittedRow = submittedRows[0];
            Assert.Equal("Provider 2", firstSubmittedRow.QuerySelector(":nth-child(1)").TextContent);
            Assert.Equal("23456", firstSubmittedRow.QuerySelector(":nth-child(2)").TextContent);
            Assert.Equal("03 May 2019", firstSubmittedRow.QuerySelector(":nth-child(3)").TextContent);

            var secondSubmittedRow = submittedRows[1];
            Assert.Equal("Provider 1", secondSubmittedRow.QuerySelector(":nth-child(1)").TextContent);
            Assert.Equal("12345", secondSubmittedRow.QuerySelector(":nth-child(2)").TextContent);
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
