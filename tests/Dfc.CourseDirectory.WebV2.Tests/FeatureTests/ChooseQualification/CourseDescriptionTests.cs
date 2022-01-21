using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.ChooseQualification;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.ChooseQualification
{
    public class CourseDescriptionTests : MvcTestBase
    {
        public CourseDescriptionTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        private async Task CourseDescription_MissingWhoThisCourseIsFor_ReturnsError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var mpx = MptxManager.CreateInstance(new FlowModel());
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);

            var get1 = await HttpClient.GetAsync(
                $"/courses/course-selected?ffiid={mpx.InstanceId}&LearnAimRef=00238422");
            
            var request = new HttpRequestMessage(HttpMethod.Post, $"/courses/add?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("WhoThisCourseIsFor", "")
                    .Add("EntryRequirements", "")
                    .Add("WhatYouWillLearn", "")
                    .Add("HowYouWillLearn", "")
                    .Add("WhatYouWillNeedToBring", "")
                    .Add("HowYouWillBeAssessed", "")
                    .Add("WhereNext", "")
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            var doc = await response.GetDocument();
            doc.AssertHasError("WhoThisCourseIsFor", "Enter who this course is for");
        }

        [Fact]
        private async Task CourseDescription_ValidPost_ReturnsRedirect()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var mpx = MptxManager.CreateInstance(new FlowModel());
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);

            var get1 = await HttpClient.GetAsync(
                $"/courses/course-selected?ffiid={mpx.InstanceId}&LearnAimRef=00238422");

            var request = new HttpRequestMessage(HttpMethod.Post, $"/courses/add?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("WhoThisCourseIsFor", "fffffff")
                    .Add("EntryRequirements", "")
                    .Add("WhatYouWillLearn", "")
                    .Add("HowYouWillLearn", "")
                    .Add("WhatYouWillNeedToBring", "")
                    .Add("HowYouWillBeAssessed", "")
                    .Add("WhereNext", "")
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        }

        [Fact]
        private async Task CourseDescription_WhoThisCourseIsForTooLargeText_ReturnsError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var mpx = MptxManager.CreateInstance(new FlowModel());
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);

            var get1 = await HttpClient.GetAsync(
                $"/courses/course-selected?ffiid={mpx.InstanceId}&LearnAimRef=00238422");

            var request = new HttpRequestMessage(HttpMethod.Post, $"/courses/add?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("WhoThisCourseIsFor", "dfdfddfdfddfdfddfdfddfdfddfdfddfdfdddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfddfdfddfdfddfdfddfdfddfdfddfdfddfdfdddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfdfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfdfddfdfddfdfdfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfddfdfd")
                    .Add("EntryRequirements", "")
                    .Add("WhatYouWillLearn", "")
                    .Add("HowYouWillLearn", "")
                    .Add("WhatYouWillNeedToBring", "")
                    .Add("HowYouWillBeAssessed", "")
                    .Add("WhereNext", "")
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            var doc = await response.GetDocument();
            doc.AssertHasError("WhoThisCourseIsFor", "Who this course is for must be 2000 characters or fewer");
        }

        [Fact]
        private async Task CourseDescription_NavigateToAddWithoutSelectingFromSearchResultsScreen_ReturnsError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var mpx = MptxManager.CreateInstance(new FlowModel());
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);

            var request = new HttpRequestMessage(HttpMethod.Post, $"/courses/add?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("WhoThisCourseIsFor", "f")
                    .Add("EntryRequirements", "")
                    .Add("WhatYouWillLearn", "")
                    .Add("HowYouWillLearn", "")
                    .Add("WhatYouWillNeedToBring", "")
                    .Add("HowYouWillBeAssessed", "")
                    .Add("WhereNext", "")
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        private async Task CourseDescription_NavigatingBackToPage_RetainsInformation()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var mpx = MptxManager.CreateInstance(new FlowModel());
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);

            var get1 = await HttpClient.GetAsync(
                $"/courses/course-selected?ffiid={mpx.InstanceId}&LearnAimRef=00238422");

            var request = new HttpRequestMessage(HttpMethod.Post, $"/courses/add?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("WhoThisCourseIsFor", "WhoThisCourseIsFor text")
                    .Add("EntryRequirements", "EntryRequirements text")
                    .Add("WhatYouWillLearn", "WhatYouWillLearn sometext")
                    .Add("HowYouWillLearn", "HowYouWillLearn this text")
                    .Add("WhatYouWillNeedToBring", "WhatYouWillNeedToBring that text")
                    .Add("HowYouWillBeAssessed", "HowYouWillBeAssessed this text")
                    .Add("WhereNext", "somewhere")
                    .ToContent()
            };
            await HttpClient.SendAsync(request);

            // Act
            var getResponse = await HttpClient.GetAsync($"/courses/add?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}");
            var doc = await getResponse.GetDocument();

            // Assert
            using (new AssertionScope())
            {
                doc.GetElementById("WhoThisCourseIsFor").TextContent.Should().Be("WhoThisCourseIsFor text");
                doc.GetElementById("EntryRequirements").TextContent.Should().Be("EntryRequirements text");
                doc.GetElementById("WhatYouWillLearn").TextContent.Should().Be("WhatYouWillLearn sometext");
                doc.GetElementById("HowYouWillLearn").TextContent.Should().Be("HowYouWillLearn this text");
                doc.GetElementById("WhatYouWillNeedToBring").TextContent.Should().Be("WhatYouWillNeedToBring that text");
                doc.GetElementById("HowYouWillBeAssessed").TextContent.Should().Be("HowYouWillBeAssessed this text");
                doc.GetElementById("WhereNext").TextContent.Should().Be("somewhere");
            }
        }
    }
}
