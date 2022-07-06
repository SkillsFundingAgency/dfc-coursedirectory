using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.ChooseQualification;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;
using FlowModel = Dfc.CourseDirectory.WebV2.Features.ChooseQualification.FlowModel;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.ChooseQualification.CheckAndPublish
{
    public class CheckAndPublishTests : MvcTestBase
    {
        public CheckAndPublishTests(CourseDirectoryApplicationFactory factory)
        : base(factory)
        {
        }

        //Navigate to check and publish page  directly renders returns error
        [Fact]
        private async Task Get_CheckAndPublishDirectly_ReturnsError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);

            // Act
            var get = await HttpClient.GetAsync($"/courses/add/check-and-publish?");


            // Assert
            get.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        private async Task Get_CheckAndPublishWithoutMptxId_ReturnsError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var mpx = MptxManager.CreateInstance(new FlowModel());
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);

            // Act
            var get = await HttpClient.GetAsync(
                $"/courses/add/check-and-publish?providerId={provider.ProviderId}");


            // Assert
            get.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }


        [Fact]
        private async Task Get_OnlineCourseRunIsValid_ReturnsValidResponse()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);

            var venueName = "Course test venue";
            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), venueName: venueName)).VenueId;

            var courseName = "CourseName";
            var providerCourseRef = "someRef";
            var startDate = new DateTime(2030, 12, 12);
            var flexibleStartDate = false;
            var nationalDelivery = false;
            IEnumerable<string> subRegionIds = new string[] { };
            var courseWebPage = "http://example.com/Course";
            var cost = "100";
            var costDescription = "some description of cost";
            var duratrion = 12;
            var durationUnit = CourseDurationUnit.Years;
            var studyMode = CourseDeliveryMode.Online;
            var attendancePattern = CourseAttendancePattern.Daytime;

            var whoThisCourseIsFor = "who this for";
            var entryRequirements = "some entry requirements";
            var whatYouWillLearn = "some who will learn";
            var howYouWillLearn = "some info on learning";
            var whatYouWillNeedToBring = "what do you need to bring";
            var howYouWillBeAssessed = "how will be assessed";
            var whereNext = "some info";

            var flowState = new FlowModel();
            var mpx = MptxManager.CreateInstance(flowState);

            flowState.SetCourseRun(
                courseName,
                providerCourseRef,
                startDate,
                flexibleStartDate,
                nationalDelivery,
                subRegionIds,
                courseWebPage,
                cost,
                costDescription,
                duratrion,
                durationUnit,
                (CourseStudyMode?)studyMode,
                attendancePattern,
                venueId
                );

            flowState.SetDeliveryMode(
                studyMode
                );

            flowState.SetCourseDescription(
                 whoThisCourseIsFor,
                 entryRequirements,
                 whatYouWillLearn,
                 howYouWillLearn,
                 whatYouWillNeedToBring,
                 howYouWillBeAssessed,
                 whereNext
               );

            var request = new HttpRequestMessage(HttpMethod.Get, $"/courses/check-and-publish?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();

            using (new AssertionScope())
            {
                //  doc.GetSummaryListValueWithKey("Start Date").Should().Be($"{startDate:d MMMM yyyy}");
                doc.GetSummaryListValueWithKey("Course Name").Should().Be(courseName);
                doc.GetSummaryListValueWithKey("Your reference").Should().Be(providerCourseRef);
                doc.GetSummaryListValueWithKey("Course webpage").Should().Be(courseWebPage);
                doc.GetSummaryListValueWithKey("Cost").Should().Be("£ " + cost);
                doc.GetSummaryListValueWithKey("Cost description").Should().Be(costDescription);
                doc.GetSummaryListValueWithKey("Duration").Should().Be(duratrion.ToString() + " " + durationUnit.ToString(""));
                doc.GetSummaryListValueWithKey("Course type").Should().Be(studyMode.ToString());

                doc.GetSummaryListValueWithKey("Who is this course for").Should().Be(whoThisCourseIsFor);
                doc.GetSummaryListValueWithKey("Entry requirements").Should().Be(entryRequirements);
                doc.GetSummaryListValueWithKey("What you'll learn").Should().Be(whatYouWillLearn);
                doc.GetSummaryListValueWithKey("How you'll learn").Should().Be(howYouWillLearn);
                doc.GetSummaryListValueWithKey("What you'll need to bring").Should().Be(whatYouWillNeedToBring);
                doc.GetSummaryListValueWithKey("How you'll be assessed").Should().Be(howYouWillBeAssessed);
                doc.GetSummaryListValueWithKey("What you can do next").Should().Be(whereNext);
            }

        }

        [Fact]
        private async Task Get_ClassroomCourseRunIsValid_ReturnsValidResponse()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);

            var venueName = "Course test venue";
            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), venueName: venueName)).VenueId;

            var courseName = "CourseName";
            var providerCourseRef = "someRef";
            var startDate = new DateTime(2030, 12, 12);
            var flexibleStartDate = false;
            var nationalDelivery = false;
            IEnumerable<string> subRegionIds = new string[] { };
            var courseWebPage = "http://example.com/Course";
            var cost = "100";
            var costDescription = "some description of cost";
            var duratrion = 12;
            var durationUnit = CourseDurationUnit.Years;
            var studyMode = CourseDeliveryMode.ClassroomBased;
            var attendancePattern = CourseAttendancePattern.Daytime;

            var whoThisCourseIsFor = "who this for";
            var entryRequirements = "some entry requirements";
            var whatYouWillLearn = "some who will learn";
            var howYouWillLearn = "some info on learning";
            var whatYouWillNeedToBring = "what do you need to bring";
            var howYouWillBeAssessed = "how will be assessed";
            var whereNext = "some info";

            var flowState = new FlowModel();
            var mpx = MptxManager.CreateInstance(flowState);

            flowState.SetCourseRun(
                courseName,
                providerCourseRef,
                startDate,
                flexibleStartDate,
                nationalDelivery,
                subRegionIds,
                courseWebPage,
                cost,
                costDescription,
                duratrion,
                durationUnit,
                (CourseStudyMode?)studyMode,
                attendancePattern,
                venueId
                );

            flowState.SetDeliveryMode(
                studyMode
                );

            flowState.SetCourseDescription(
                 whoThisCourseIsFor,
                 entryRequirements,
                 whatYouWillLearn,
                 howYouWillLearn,
                 whatYouWillNeedToBring,
                 howYouWillBeAssessed,
                 whereNext
               );

            var Delivery = "";

            if (studyMode == CourseDeliveryMode.ClassroomBased)
            {
                Delivery = "Classroom based";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"/courses/check-and-publish?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();

            using (new AssertionScope())
            {
                //doc.GetSummaryListValueWithKey("Start Date").Should().Be($"{startDate:d MMMM yyyy}");
                doc.GetSummaryListValueWithKey("Venue").Should().Be(venueName);
                doc.GetSummaryListValueWithKey("Course Name").Should().Be(courseName);
                doc.GetSummaryListValueWithKey("Your reference").Should().Be(providerCourseRef);
                doc.GetSummaryListValueWithKey("Cost").Should().Be("£ " + cost);
                doc.GetSummaryListValueWithKey("Cost description").Should().Be(costDescription);
                doc.GetSummaryListValueWithKey("Duration").Should().Be(duratrion.ToString() + " " + durationUnit.ToString(""));
                doc.GetSummaryListValueWithKey("Course type").Should().Be(Delivery);
                doc.GetSummaryListValueWithKey("Course webpage").Should().Be(courseWebPage);
                doc.GetSummaryListValueWithKey("Attendance").Should().Be(attendancePattern.ToString());

                doc.GetSummaryListValueWithKey("Who is this course for").Should().Be(whoThisCourseIsFor);
                doc.GetSummaryListValueWithKey("Entry requirements").Should().Be(entryRequirements);
                doc.GetSummaryListValueWithKey("What you'll learn").Should().Be(whatYouWillLearn);
                doc.GetSummaryListValueWithKey("How you'll learn").Should().Be(howYouWillLearn);
                doc.GetSummaryListValueWithKey("What you'll need to bring").Should().Be(whatYouWillNeedToBring);
                doc.GetSummaryListValueWithKey("How you'll be assessed").Should().Be(howYouWillBeAssessed);
                doc.GetSummaryListValueWithKey("What you can do next").Should().Be(whereNext);
            }

        }


        [Fact]
        private async Task Get_WorkbasedCourseRunIsValid_ReturnsValidResponse()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);

            var venueName = "Course test venue";
            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), venueName: venueName)).VenueId;

            var courseName = "CourseName";
            var providerCourseRef = "someRef";
            var startDate = new DateTime(2030, 12, 12);
            var flexibleStartDate = false;
            var nationalDelivery = false;
            IEnumerable<string> subRegionIds = new string[] { };
            var courseWebPage = "http://example.com/Course";
            var cost = "100";
            var costDescription = "some description of cost";
            var duratrion = 12;
            var durationUnit = CourseDurationUnit.Years;
            var studyMode = CourseDeliveryMode.WorkBased;
            var attendancePattern = CourseAttendancePattern.Daytime;

            var whoThisCourseIsFor = "who this for";
            var entryRequirements = "some entry requirements";
            var whatYouWillLearn = "some who will learn";
            var howYouWillLearn = "some info on learning";
            var whatYouWillNeedToBring = "what do you need to bring";
            var howYouWillBeAssessed = "how will be assessed";
            var whereNext = "some info";

            var flowState = new FlowModel();
            var mpx = MptxManager.CreateInstance(flowState);

            flowState.SetCourseRun(
                courseName,
                providerCourseRef,
                startDate,
                flexibleStartDate,
                nationalDelivery,
                subRegionIds,
                courseWebPage,
                cost,
                costDescription,
                duratrion,
                durationUnit,
                (CourseStudyMode?)studyMode,
                attendancePattern,
                venueId
                );

            flowState.SetDeliveryMode(
                studyMode
                );

            flowState.SetCourseDescription(
                 whoThisCourseIsFor,
                 entryRequirements,
                 whatYouWillLearn,
                 howYouWillLearn,
                 whatYouWillNeedToBring,
                 howYouWillBeAssessed,
                 whereNext
               );

            var Delivery = "";

            if (studyMode == CourseDeliveryMode.WorkBased)
            {
                Delivery = "Work based";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"/courses/check-and-publish?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();

            using (new AssertionScope())
            {
                //  doc.GetSummaryListValueWithKey("Start Date").Should().Be($"{startDate:d MMMM yyyy}");
                doc.GetSummaryListValueWithKey("Course Name").Should().Be(courseName);
                doc.GetSummaryListValueWithKey("Your reference").Should().Be(providerCourseRef);
                //   doc.GetSummaryListValueWithKey("National delivery mode").Should().Be(nationalDelivery.ToString());
                // doc.GetSummaryListValueWithKey("Regions ").Should().Be(subRegionIds.ToString());
                doc.GetSummaryListValueWithKey("Course webpage").Should().Be(courseWebPage);
                doc.GetSummaryListValueWithKey("Cost").Should().Be("£ " + cost);
                doc.GetSummaryListValueWithKey("Cost description").Should().Be(costDescription);
                doc.GetSummaryListValueWithKey("Duration").Should().Be(duratrion.ToString() + " " + durationUnit.ToString(""));
                doc.GetSummaryListValueWithKey("Course type").Should().Be(Delivery);

                doc.GetSummaryListValueWithKey("Who is this course for").Should().Be(whoThisCourseIsFor);
                doc.GetSummaryListValueWithKey("Entry requirements").Should().Be(entryRequirements);
                doc.GetSummaryListValueWithKey("What you'll learn").Should().Be(whatYouWillLearn);
                doc.GetSummaryListValueWithKey("How you'll learn").Should().Be(howYouWillLearn);
                doc.GetSummaryListValueWithKey("What you'll need to bring").Should().Be(whatYouWillNeedToBring);
                doc.GetSummaryListValueWithKey("How you'll be assessed").Should().Be(howYouWillBeAssessed);
                doc.GetSummaryListValueWithKey("What you can do next").Should().Be(whereNext);
            }

        }

        /*
        [Fact]
        private async Task Post_OnlineCourseRunIsValid_ReturnsValidResponse()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);

            var venueName = "Course test venue";
            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), venueName: venueName)).VenueId;

            var courseName = "CourseName";
            var providerCourseRef = "someRef";
            var startDate = new DateTime(2030, 12, 12);
            var flexibleStartDate = false;
            var nationalDelivery = false;
            IEnumerable<string> subRegionIds = new string[] { };
            var courseWebPage = "http://example.com/Course";
            var cost = "100";
            var costDescription = "some description of cost";
            var duratrion = 12;
            var durationUnit = CourseDurationUnit.Years;
            var studyMode = CourseDeliveryMode.Online;
            var attendancePattern = CourseAttendancePattern.Daytime;

            var whoThisCourseIsFor = "who this for";
            var entryRequirements = "some entry requirements";
            var whatYouWillLearn = "some who will learn";
            var howYouWillLearn = "some info on learning";
            var whatYouWillNeedToBring = "what do you need to bring";
            var howYouWillBeAssessed = "how will be assessed";
            var whereNext = "some info";

            var flowState = new FlowModel();
            var mpx = MptxManager.CreateInstance(flowState);

            flowState.SetCourseRun(
                courseName,
                providerCourseRef,
                startDate,
                flexibleStartDate,
                nationalDelivery,
                subRegionIds,
                courseWebPage,
                cost,
                costDescription,
                duratrion,
                durationUnit,
                (CourseStudyMode?)studyMode,
                attendancePattern,
                venueId
                );

            flowState.SetDeliveryMode(
                studyMode
                );

            flowState.SetCourseDescription(
                 whoThisCourseIsFor,
                 entryRequirements,
                 whatYouWillLearn,
                 howYouWillLearn,
                 whatYouWillNeedToBring,
                 howYouWillBeAssessed,
                 whereNext
               );

            var request = new HttpRequestMessage(HttpMethod.Get, $"/courses/add/check-and-publish?ffiid={mpx.InstanceId}&providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
 
        } */

    }
}
