using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.ChooseQualification;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.ChooseQualification
{
    public class CourseRunTests : MvcTestBase
    {
        public CourseRunTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }


        #region generic tests
        //Navigate to courserun directly renders returns error
        [Fact]
        private async Task Get_CourseRunDirectly_ReturnsError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);

            // Act
            var get = await HttpClient.GetAsync(
                $"/courses/add-courserun?deliveryMode=classroom");


            // Assert
            get.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        private async Task Get_CourseRunWithoutMptxId_ReturnsError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var mpx = MptxManager.CreateInstance(new FlowModel());
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);

            // Act
            var get = await HttpClient.GetAsync(
                $"/courses/add-courserun?deliveryMode=classroom&providerId={provider.ProviderId}");


            // Assert
            get.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Theory]
        [InlineData(CourseDeliveryMode.ClassroomBased)]
        [InlineData(CourseDeliveryMode.WorkBased)]
        [InlineData(CourseDeliveryMode.Online)]
        private async Task Post_InvalidCourseWebpage_ReturnsError(CourseDeliveryMode deliveryMode)
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var mpx = MptxManager.CreateInstance(new FlowModel());
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);
            await HttpClient.GetAsync(
                $"/courses/course-selected?ffiid={mpx.InstanceId}&LearnAimRef=00238422");

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
            var postDeliveryRequest = new HttpRequestMessage(HttpMethod.Post, $"/courses/add/delivery?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("DeliveryMode", CourseDeliveryMode.ClassroomBased)
                    .ToContent()
            };
            await HttpClient.SendAsync(postDeliveryRequest);

            // Act
            var postCourseRun = new HttpRequestMessage(HttpMethod.Post, $"/courses/add-courserun?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                .Add("DeliveryMode", deliveryMode)
                .Add("CourseName", "Some Details")
                .Add("ProviderCourseRef", "someEf")
                .Add("StartDate", "")
                .Add("FlexibleStartDate", "true")
                .Add("NationalDelivery", "")
                .Add("SubRegionIds", "")
                .Add("CourseWebPage", "someInvalid-com")
                .Add("Cost", "1000")
                .Add("CostDescription", "")
                .Add("Duration", "12")
                .Add("DurationUnit", "")
                .Add("StudyMode", "")
                .Add("AttendancePattern", "")
                .Add("VenueId", "")
                .ToContent()
            };
            var postCourseRunResponse = await HttpClient.SendAsync(postCourseRun);

            // Assert
            postCourseRunResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var doc = await postCourseRunResponse.GetDocument();
            using (new AssertionScope())
            {
                doc.AssertHasError("CourseWebPage", "Course webpage must be a real website");
            }
        }
        #endregion

        #region online tests
        [Fact]
        private async Task Post_OnlineCourseRunIsValid_ReturnsRedirectToCheckAndPublish()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var mpx = MptxManager.CreateInstance(new FlowModel());
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);


            await HttpClient.GetAsync(
                $"/courses/course-selected?ffiid={mpx.InstanceId}&LearnAimRef=00238422");

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
            var postDeliveryRequest = new HttpRequestMessage(HttpMethod.Post, $"/courses/add/delivery?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("DeliveryMode", CourseDeliveryMode.Online)
                    .ToContent()
            };
            await HttpClient.SendAsync(postDeliveryRequest);

            // Act
            var postCourseRun = new HttpRequestMessage(HttpMethod.Post, $"/courses/add-courserun?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                .Add("DeliveryMode", CourseDeliveryMode.Online)
                .Add("CourseName", "Some Details")
                .Add("ProviderCourseRef", "someEf")
                .Add("StartDate", "")
                .Add("FlexibleStartDate", "true")
                .Add("NationalDelivery", "")
                .Add("SubRegionIds", "")
                .Add("CourseWebPage", "")
                .Add("Cost", "1000")
                .Add("CostDescription", "")
                .Add("Duration", "12")
                .Add("DurationUnit", CourseDurationUnit.Years)
                .Add("StudyMode", "")
                .Add("AttendancePattern", "")
                .Add("VenueId", "")
                .ToContent()
            };
            var postCourseRunResponse = await HttpClient.SendAsync(postCourseRun);

            // Assert
            postCourseRunResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
            postCourseRunResponse.Headers.Location.Should().Be($"/courses/add/check-and-publish?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}");
        }


        [Fact]
        private async Task Post_OnlineCourseRunWithNoDetails_ReturnsErrors()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var mpx = MptxManager.CreateInstance(new FlowModel());
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "VENUE1");

            await HttpClient.GetAsync(
                $"/courses/course-selected?ffiid={mpx.InstanceId}&LearnAimRef=00238422");

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
            var postDeliveryRequest = new HttpRequestMessage(HttpMethod.Post, $"/courses/add/delivery?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("DeliveryMode", CourseDeliveryMode.ClassroomBased)
                    .ToContent()
            };
            await HttpClient.SendAsync(postDeliveryRequest);

            // Act
            var postCourseRun = new HttpRequestMessage(HttpMethod.Post, $"/courses/add-courserun?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                .Add("DeliveryMode", CourseDeliveryMode.Online)
                .Add("CourseName", "")
                .Add("ProviderCourseRef", "")
                .Add("StartDate", "")
                .Add("FlexibleStartDate", "")
                .Add("NationalDelivery", "")
                .Add("SubRegionIds", "")
                .Add("CourseWebPage", "")
                .Add("Cost", "")
                .Add("CostDescription", "")
                .Add("Duration", "")
                .Add("DurationUnit", "")
                .Add("StudyMode", "")
                .Add("AttendancePattern", "")
                .Add("VenueId", "")
                .ToContent()
            };
            var postCourseRunResponse = await HttpClient.SendAsync(postCourseRun);

            // Assert
            postCourseRunResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var doc = await postCourseRunResponse.GetDocument();
            using (new AssertionScope())
            {
                doc.AssertHasError("CourseName", "Enter a Course name");
                doc.AssertHasError("Duration", "Enter duration");
                doc.AssertHasError("DurationUnit", "Enter a duration unit");
                doc.AssertHasError("FlexibleStartDate", "Enter start date");
                doc.AssertHasError("Cost", "Enter a cost or cost description");
            }

        }
        #endregion

        #region classroom tests
        [Fact]
        private async Task Post_ClassroomCourseRunIsValid_ReturnsRedirectToCheckAndPublish()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var mpx = MptxManager.CreateInstance(new FlowModel());
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "VENUE1");

            await HttpClient.GetAsync(
                $"/courses/course-selected?ffiid={mpx.InstanceId}&LearnAimRef=00238422");

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
            var postDeliveryRequest = new HttpRequestMessage(HttpMethod.Post, $"/courses/add/delivery?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("DeliveryMode", CourseDeliveryMode.ClassroomBased)
                    .ToContent()
            };
            await HttpClient.SendAsync(postDeliveryRequest);

            // Act
            var postCourseRun = new HttpRequestMessage(HttpMethod.Post, $"/courses/add-courserun?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                .Add("DeliveryMode", CourseDeliveryMode.ClassroomBased)
                .Add("CourseName", "Some Details")
                .Add("ProviderCourseRef", "someEf")
                .Add("StartDate", "")
                .Add("FlexibleStartDate", "true")
                .Add("NationalDelivery", "")
                .Add("SubRegionIds", "")
                .Add("CourseWebPage", "")
                .Add("Cost", "1000")
                .Add("CostDescription", "")
                .Add("Duration", "12")
                .Add("DurationUnit", CourseDurationUnit.Years)
                .Add("StudyMode", CourseStudyMode.FullTime)
                .Add("AttendancePattern", CourseAttendancePattern.Evening)
                .Add("VenueId", venue.VenueId)
                .ToContent()
            };
            var postCourseRunResponse = await HttpClient.SendAsync(postCourseRun);

            // Assert
            postCourseRunResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
            postCourseRunResponse.Headers.Location.Should().Be($"/courses/add/check-and-publish?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}");
        }

        [Fact]
        private async Task Post_ClassroomCourseRunWithNoVenue_ReturnsError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var mpx = MptxManager.CreateInstance(new FlowModel());
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);
            await HttpClient.GetAsync(
                $"/courses/course-selected?ffiid={mpx.InstanceId}&LearnAimRef=00238422");

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
            var postDeliveryRequest = new HttpRequestMessage(HttpMethod.Post, $"/courses/add/delivery?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("DeliveryMode", CourseDeliveryMode.ClassroomBased)
                    .ToContent()
            };
            await HttpClient.SendAsync(postDeliveryRequest);

            // Act
            var postCourseRun = new HttpRequestMessage(HttpMethod.Post, $"/courses/add-courserun?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                .Add("DeliveryMode", CourseDeliveryMode.ClassroomBased)
                .Add("CourseName", "Some Details")
                .Add("ProviderCourseRef", "someEf")
                .Add("StartDate", "")
                .Add("FlexibleStartDate", "true")
                .Add("NationalDelivery", "")
                .Add("SubRegionIds", "")
                .Add("CourseWebPage", "")
                .Add("Cost", "1000")
                .Add("CostDescription", "")
                .Add("Duration", "12")
                .Add("DurationUnit", CourseDurationUnit.Years)
                .Add("StudyMode", CourseStudyMode.FullTime)
                .Add("AttendancePattern", CourseAttendancePattern.Evening)
                .Add("VenueId", "")
                .ToContent()
            };
            var postCourseRunResponse = await HttpClient.SendAsync(postCourseRun);

            // Assert
            postCourseRunResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var doc = await postCourseRunResponse.GetDocument();
            doc.AssertHasError("VenueId", "Select a venue");
        }

        [Fact]
        private async Task Post_ClassroomCourseRunWithNoDetails_ReturnsErrors()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var mpx = MptxManager.CreateInstance(new FlowModel());
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "VENUE1");

            await HttpClient.GetAsync(
                $"/courses/course-selected?ffiid={mpx.InstanceId}&LearnAimRef=00238422");

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
            var postDeliveryRequest = new HttpRequestMessage(HttpMethod.Post, $"/courses/add/delivery?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("DeliveryMode", CourseDeliveryMode.ClassroomBased)
                    .ToContent()
            };
            await HttpClient.SendAsync(postDeliveryRequest);

            // Act
            var postCourseRun = new HttpRequestMessage(HttpMethod.Post, $"/courses/add-courserun?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                .Add("DeliveryMode", CourseDeliveryMode.ClassroomBased)
                .Add("CourseName", "")
                .Add("ProviderCourseRef", "")
                .Add("StartDate", "")
                .Add("FlexibleStartDate", "")
                .Add("NationalDelivery", "")
                .Add("SubRegionIds", "")
                .Add("CourseWebPage", "")
                .Add("Cost", "")
                .Add("CostDescription", "")
                .Add("Duration", "")
                .Add("DurationUnit", "")
                .Add("StudyMode", "")
                .Add("AttendancePattern", "")
                .Add("VenueId", "")
                .ToContent()
            };
            var postCourseRunResponse = await HttpClient.SendAsync(postCourseRun);

            // Assert
            postCourseRunResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var doc = await postCourseRunResponse.GetDocument();
            using (new AssertionScope())
            {
                doc.AssertHasError("CourseName", "Enter a Course name");
                doc.AssertHasError("FlexibleStartDate", "Enter start date");
                doc.AssertHasError("Cost", "Enter a cost or cost description");
                doc.AssertHasError("Duration", "Enter duration");
                doc.AssertHasError("DurationUnit", "Enter a duration unit");
                doc.AssertHasError("AttendancePattern", "Select an attendance pattern");
                doc.AssertHasError("StudyMode", "Select course hours");
                doc.AssertHasError("VenueId", "Select a venue");
            }
        }
        #endregion

        #region work based tests
        [Fact]
        private async Task Post_WorkbasedCourseRunIsValid_ReturnsRedirectToCheckAndPublish()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var mpx = MptxManager.CreateInstance(new FlowModel());
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);

            await HttpClient.GetAsync(
                $"/courses/course-selected?ffiid={mpx.InstanceId}&LearnAimRef=00238422");

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
            var postDeliveryRequest = new HttpRequestMessage(HttpMethod.Post, $"/courses/add/delivery?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("DeliveryMode", CourseDeliveryMode.WorkBased)
                    .ToContent()
            };
            await HttpClient.SendAsync(postDeliveryRequest);

            // Act
            var postCourseRun = new HttpRequestMessage(HttpMethod.Post, $"/courses/add-courserun?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                .Add("DeliveryMode", CourseDeliveryMode.WorkBased)
                .Add("CourseName", "Some Details")
                .Add("ProviderCourseRef", "someEf")
                .Add("StartDate", "")
                .Add("FlexibleStartDate", "true")
                .Add("NationalDelivery", "true")
                .Add("SubRegionIds", "")
                .Add("CourseWebPage", "")
                .Add("Cost", "1000")
                .Add("CostDescription", "")
                .Add("Duration", "12")
                .Add("DurationUnit", CourseDurationUnit.Years)
                .Add("StudyMode", "")
                .Add("AttendancePattern", "")
                .Add("VenueId","")
                .ToContent()
            };
            var postCourseRunResponse = await HttpClient.SendAsync(postCourseRun);

            // Assert
            postCourseRunResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
            postCourseRunResponse.Headers.Location.Should().Be($"/courses/add/check-and-publish?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}");
        }

        [Fact]
        private async Task Post_WorkbasedCourseRunWithNoDetails_ReturnsErrors()
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
            var postDeliveryRequest = new HttpRequestMessage(HttpMethod.Post, $"/courses/add/delivery?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("DeliveryMode", CourseDeliveryMode.WorkBased)
                    .ToContent()
            };
            await HttpClient.SendAsync(postDeliveryRequest);

            // Act
            var postCourseRun = new HttpRequestMessage(HttpMethod.Post, $"/courses/add-courserun?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                .Add("DeliveryMode", CourseDeliveryMode.WorkBased)
                .Add("CourseName", "")
                .Add("ProviderCourseRef", "")
                .Add("StartDate", "")
                .Add("FlexibleStartDate", "")
                .Add("NationalDelivery", "")
                .Add("SubRegionIds", "")
                .Add("CourseWebPage", "")
                .Add("Cost", "")
                .Add("CostDescription", "")
                .Add("Duration", "")
                .Add("DurationUnit", "")
                .Add("StudyMode", "")
                .Add("AttendancePattern", "")
                .Add("VenueId", "")
                .ToContent()
            };
            var postCourseRunResponse = await HttpClient.SendAsync(postCourseRun);

            // Assert
            postCourseRunResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var doc = await postCourseRunResponse.GetDocument();
            using (new AssertionScope())
            {
                doc.AssertHasError("CourseName", "Enter a Course name");
                doc.AssertHasError("FlexibleStartDate", "Enter start date");
                doc.AssertHasError("Cost", "Enter a cost or cost description");
                doc.AssertHasError("Duration", "Enter duration");
                doc.AssertHasError("DurationUnit", "Enter a duration unit");
                doc.AssertHasError("NationalDelivery", "Select National or at least one sub region");
            }
        }

        [Fact]
        private async Task Post_WorkbasedCourseRunRegionalWithNoRegions_ReturnsError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var mpx = MptxManager.CreateInstance(new FlowModel());
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);
            await HttpClient.GetAsync(
                $"/courses/course-selected?ffiid={mpx.InstanceId}&LearnAimRef=00238422");

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
            var postDeliveryRequest = new HttpRequestMessage(HttpMethod.Post, $"/courses/add/delivery?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("DeliveryMode", CourseDeliveryMode.WorkBased)
                    .ToContent()
            };
            await HttpClient.SendAsync(postDeliveryRequest);

            // Act
            var postCourseRun = new HttpRequestMessage(HttpMethod.Post, $"/courses/add-courserun?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                .Add("DeliveryMode", CourseDeliveryMode.WorkBased)
                .Add("CourseName", "Some Details")
                .Add("ProviderCourseRef", "someEf")
                .Add("StartDate", "")
                .Add("FlexibleStartDate", "true")
                .Add("NationalDelivery", "false")
                .Add("SubRegionIds", "") //Not supplying regions will return error
                .Add("CourseWebPage", "")
                .Add("Cost", "1000")
                .Add("CostDescription", "")
                .Add("Duration", "12")
                .Add("DurationUnit", CourseDurationUnit.Years)
                .Add("StudyMode", "")
                .Add("AttendancePattern", "")
                .Add("VenueId", "")
                .ToContent()
            };
            var postCourseRunResponse = await HttpClient.SendAsync(postCourseRun);

            // Assert
            postCourseRunResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var doc = await postCourseRunResponse.GetDocument();
        }

        [Fact]
        private async Task Post_WorkbasedCourseRunWithRegionsIsValid_ReturnsRedirectToCheckAndPublish()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var mpx = MptxManager.CreateInstance(new FlowModel());
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);
            await HttpClient.GetAsync(
                $"/courses/course-selected?ffiid={mpx.InstanceId}&LearnAimRef=00238422");

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
            var postDeliveryRequest = new HttpRequestMessage(HttpMethod.Post, $"/courses/add/delivery?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("DeliveryMode", CourseDeliveryMode.WorkBased)
                    .ToContent()
            };
            await HttpClient.SendAsync(postDeliveryRequest);

            // Act
            var postCourseRun = new HttpRequestMessage(HttpMethod.Post, $"/courses/add-courserun?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                .Add("DeliveryMode", CourseDeliveryMode.WorkBased)
                .Add("CourseName", "Some Details")
                .Add("ProviderCourseRef", "someEf")
                .Add("StartDate", "")
                .Add("FlexibleStartDate", "true")
                .Add("NationalDelivery", "false")
                .Add("SubRegionIds", new string[] { "E06000015" })
                .Add("CourseWebPage", "")
                .Add("Cost", "1000")
                .Add("CostDescription", "")
                .Add("Duration", "12")
                .Add("DurationUnit", CourseDurationUnit.Years)
                .Add("StudyMode", "")
                .Add("AttendancePattern", "")
                .Add("VenueId", "")
                .ToContent()
            };
            var postCourseRunResponse = await HttpClient.SendAsync(postCourseRun);

            // Assert
            postCourseRunResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
            postCourseRunResponse.Headers.Location.Should().Be($"/courses/add/check-and-publish?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}");
        }
        #endregion
    }
}
