using Dfc.CourseDirectory.WebV2.Models;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.ApprenticeshipQA
{
    public class QAStatusReportTests : TestBase
    {
        public QAStatusReportTests(CourseDirectoryApplicationFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task Get_QAReport_Returns_Returns_No_Results()
        {
            // Arrange
            // nothing to arrange

            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/qareport");
            var results = await response.AsCsvListOf<QAStatusReport>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(results);
        }

        [Fact]
        public async Task Get_QAReport_Returns_One_Result()
        {
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);

            var apprenticeshipId = await TestData.CreateApprenticeship(ukprn);

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: true,
                apprenticeshipAssessmentsPassed: null,
                passed: null,
                lastAssessedByUserId: User.UserId.ToString(),
                lastAssessedOn: Clock.UtcNow);

            await User.AsHelpdesk();

            var requestContentBuilder = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", true)
                .Add("ComplianceComments", "Some compliance feedback")
                .Add("StylePassed", true);

            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/qareport");
            var results =  await response.AsCsvListOf<QAStatusReport>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            //Assert.Collection(results);
        }
    }
}

//Providers
//ApprenticeshipQAUnableToCompleteInfo
//ApprenticeshipQASubmissions
//ApprenticeshipQASubmissionProviderAssessments
//Users