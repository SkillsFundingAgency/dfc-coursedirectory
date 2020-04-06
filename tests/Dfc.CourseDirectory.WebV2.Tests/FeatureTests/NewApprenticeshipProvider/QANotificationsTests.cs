using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.NewApprenticeshipProvider
{
    public class QANotificationsTests : MvcTestBase
    {
        public QANotificationsTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(ApprenticeshipQAStatus.NotStarted, "pttcd-new-apprenticeship-provider__qa-notifications-not-started")]
        [InlineData(ApprenticeshipQAStatus.Submitted, "pttcd-new-apprenticeship-provider__qa-notifications-submitted")]
        [InlineData(ApprenticeshipQAStatus.Submitted | ApprenticeshipQAStatus.UnableToComplete, "pttcd-new-apprenticeship-provider__qa-notifications-submitted")]
        [InlineData(ApprenticeshipQAStatus.InProgress, "pttcd-new-apprenticeship-provider__qa-notifications-submitted")]
        [InlineData(ApprenticeshipQAStatus.Failed, "pttcd-new-apprenticeship-provider__qa-notifications-failed")]
        [InlineData(ApprenticeshipQAStatus.Passed, "pttcd-new-apprenticeship-provider__qa-notifications-passed")]
        public async Task RendersCorrectMessage(ApprenticeshipQAStatus qaStatus, string expectedNotificationId)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: qaStatus);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.GetAsync("QANotificationsTests");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();

            var notificationElements = doc.QuerySelectorAll("div")
                .Where(e => e.Id?.StartsWith("pttcd-new-apprenticeship-provider__qa-notifications-") ?? false);

            if (expectedNotificationId == null)
            {
                Assert.Empty(notificationElements);
            }
            else
            {
                var notification = Assert.Single(notificationElements);
                Assert.Equal(expectedNotificationId, notification.Id);
            }
        }

        [Theory]
        [InlineData(ApprenticeshipQAStatus.NotStarted, "pttcd-new-apprenticeship-provider__qa-notifications-not-started")]
        [InlineData(ApprenticeshipQAStatus.Submitted, "pttcd-new-apprenticeship-provider__qa-notifications-submitted")]
        [InlineData(ApprenticeshipQAStatus.Submitted | ApprenticeshipQAStatus.UnableToComplete, "pttcd-new-apprenticeship-provider__qa-notifications-submitted")]
        [InlineData(ApprenticeshipQAStatus.InProgress, "pttcd-new-apprenticeship-provider__qa-notifications-submitted")]
        [InlineData(ApprenticeshipQAStatus.Failed, "pttcd-new-apprenticeship-provider__qa-notifications-failed")]
        [InlineData(ApprenticeshipQAStatus.Passed, "pttcd-new-apprenticeship-provider__qa-notifications-passed")]
        public async Task FEOnlyProviderRendersNoMessage(ApprenticeshipQAStatus qaStatus, string notificationId)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                apprenticeshipQAStatus: qaStatus,
                providerType: ProviderType.FE);

            await User.AsProviderUser(providerId, ProviderType.FE);

            // Act
            var response = await HttpClient.GetAsync("QANotificationsTests");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            Assert.Null(doc.GetElementById(notificationId));
        }
    }

    public class QANotificationsTestsController : Controller
    {
        [HttpGet("QANotificationsTests")]
        public IActionResult Get() => View("~/FeatureTests/NewApprenticeshipProvider/QANotificationsTests.cshtml");
    }
}
