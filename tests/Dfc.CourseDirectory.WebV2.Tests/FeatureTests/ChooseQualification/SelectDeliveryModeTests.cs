using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.ChooseQualification;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.ChooseQualification
{
    public class SelectDeliveryModeTests : MvcTestBase
    {
        public SelectDeliveryModeTests(CourseDirectoryApplicationFactory factory) : base(factory)
        {
        }

        [Theory]
        [InlineData(CourseDeliveryMode.ClassroomBased)]
        [InlineData(CourseDeliveryMode.Online)]
        [InlineData(CourseDeliveryMode.WorkBased)]
        private async Task Post_SelectDeliveryMode_RedirectsToCourseRun(CourseDeliveryMode deliveryMode)
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var mpx = MptxManager.CreateInstance(new FlowModel());
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);

            var get1 = await HttpClient.GetAsync(
                $"/courses/choose-qualification/course-selected?ffiid={mpx.InstanceId}&LearnAimRef=00238422");

            var request = new HttpRequestMessage(HttpMethod.Post, $"/courses/add?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("WhoThisCourseIsFor", "Some Details")
                    .Add("EntryRequirements", "")
                    .Add("WhatYouWillLearn", "")
                    .Add("HowYouWillLearn", "")
                    .Add("WhatYouWillNeedToBring", "")
                    .Add("HowYouWillBeAssessed", "")
                    .Add("WhereNext", "")
                    .ToContent()
            };

            await HttpClient.SendAsync(request);

            // Act
            var postDeliveryRequest = new HttpRequestMessage(HttpMethod.Post, $"/courses/add/delivery?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("DeliveryMode", deliveryMode)
                    .ToContent()
            };
            var postDeliveryResponse = await HttpClient.SendAsync(postDeliveryRequest);

            // Assert
            postDeliveryResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
            postDeliveryResponse.Headers.Location.Should().Be($"/courses/add-courserun?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}");
        }

        [Fact]
        private async Task Post_SelectDeliveryModeNoSelection_ReturnsError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var mpx = MptxManager.CreateInstance(new FlowModel());
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);
            var request = new HttpRequestMessage(HttpMethod.Post, $"/courses/add?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("WhoThisCourseIsFor", "Some Details")
                    .Add("EntryRequirements", "")
                    .Add("WhatYouWillLearn", "")
                    .Add("HowYouWillLearn", "")
                    .Add("WhatYouWillNeedToBring", "")
                    .Add("HowYouWillBeAssessed", "")
                    .Add("WhereNext", "")
                    .ToContent()
            };
            await HttpClient.SendAsync(request);

            // Act
            var postDeliveryRequest = new HttpRequestMessage(HttpMethod.Post, $"/courses/add/delivery?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .ToContent()
            };
            var postDeliveryResponse = await HttpClient.SendAsync(postDeliveryRequest);

            // Assert
            postDeliveryResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var doc = await postDeliveryResponse.GetDocument();
            doc.AssertHasError("DeliveryMode", "Select how the course will be delivered");
        }

        [Fact]
        private async Task Get_CancelLinkNavigatesToCourseDescription_ReturnsError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var mpx = MptxManager.CreateInstance(new FlowModel());
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);
            var request = new HttpRequestMessage(HttpMethod.Post, $"/courses/add?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("WhoThisCourseIsFor", "Some Details")
                    .Add("EntryRequirements", "")
                    .Add("WhatYouWillLearn", "")
                    .Add("HowYouWillLearn", "")
                    .Add("WhatYouWillNeedToBring", "")
                    .Add("HowYouWillBeAssessed", "")
                    .Add("WhereNext", "")
                    .ToContent()
            };
            await HttpClient.SendAsync(request);

            // Act
            var postDeliveryRequest = new HttpRequestMessage(HttpMethod.Post, $"/courses/add/delivery?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .ToContent()
            };
            var postDeliveryResponse = await HttpClient.SendAsync(postDeliveryRequest);

            // Assert
            var doc = await postDeliveryResponse.GetDocument();
            var btn = doc.GetElementByTestId("CancelButton");
            btn.Attributes["href"].Value.Should().Be($"/courses/add?providerId={provider.ProviderId}&ffiid={mpx.InstanceId}");
        }
    }
}
