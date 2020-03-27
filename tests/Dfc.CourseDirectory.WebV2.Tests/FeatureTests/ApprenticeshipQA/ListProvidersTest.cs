using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.ApprenticeshipQA
{
    public class ListProvidersTest : TestBase
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

            var provider1Ukprn = 12345;
            var provider1UserId = $"user-{provider1Ukprn}";
            var provider1Id = await TestData.CreateProvider(
                ukprn: provider1Ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: Models.ApprenticeshipQAStatus.Submitted);
            await TestData.CreateUser(provider1UserId, "guy@provider1.com", "Provider 1", "User", provider1Id);
            var provider1ApprenticeshipId = await TestData.CreateApprenticeship(providerUkprn: provider1Ukprn);
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
                apprenticeshipQAStatus: Models.ApprenticeshipQAStatus.Submitted);
            await TestData.CreateUser(provider2UserId, "guy@provider2.com", "Provider 2", "User", provider2Id);
            var provider2ApprenticeshipId = await TestData.CreateApprenticeship(providerUkprn: provider2Ukprn);
            await TestData.CreateApprenticeshipQASubmission(
                provider2Id,
                submittedOn: new DateTime(2019, 5, 3, 15, 01, 23),
                submittedByUserId: provider2UserId,
                providerMarketingInformation: "Provider 2 overview",
                apprenticeshipIds: new[] { provider2ApprenticeshipId });

            // TODO Add more here once we have a way of modelling other statuses

            // Act
            var response = await HttpClient.GetAsync("apprenticeship-qa");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();

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
