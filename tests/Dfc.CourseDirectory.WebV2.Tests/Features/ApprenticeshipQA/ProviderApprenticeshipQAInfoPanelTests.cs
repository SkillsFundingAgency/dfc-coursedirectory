using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Models;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.Features.ApprenticeshipQA
{
    public class ProviderApprenticeshipQAInfoPanelTests : TestBase
    {
        public ProviderApprenticeshipQAInfoPanelTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task RendersExpectedOutput()
        {
            // Arrange
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person");

            var apprenticeshipId = await TestData.CreateApprenticeship(ukprn);

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await TestData.CreateUserSignIn(providerUserId, new DateTime(2018, 4, 12, 11, 30, 10, DateTimeKind.Utc));

            await User.AsHelpdesk();

            // Act
            // TODO Find a way to host this in its own URL for testing purposes
            var response = await HttpClient.GetAsync($"apprenticeship-qa/provider-assessments/{providerId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();

            //
        }
    }
}
