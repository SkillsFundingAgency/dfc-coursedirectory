using Dfc.CourseDirectory.WebV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.NewApprenticeshipProvider
{
    public class HidePassedNotificationTests : TestBase
    {
        public HidePassedNotificationTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task That_Invalid_Submission_Returns_Bad_Request()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.Passed);
            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/hide-passed-notification?providerId={providerId}&returnUrl=/",null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        //{
        //Create provider
        //call hide notication
        //assert bad request
        //assert bad request

        //create provider
        //create assessment
        //call hide notification on failed QA
        //assert bad request

        //create provider
        //create assessment
        //pass assessment
        //call hide notication
        //assert bad request

    }
}
