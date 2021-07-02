using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.Models;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Courses
{
    public class DownloadTests : MvcTestBase
    {
        public DownloadTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_DownloadsValidFile()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerName: "Test Provider");
            Clock.UtcNow = new DateTime(2021, 4, 9, 13, 0, 0);
            await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());

            // Act
            var response = await HttpClient.GetAsync($"/data-upload/courses/download?providerId={provider.ProviderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.FileName.Should().Be("\"Test Provider_courses_202104091300.csv\"");

            using var responseBody = await response.Content.ReadAsStreamAsync();
            using var responseBodyReader = new StreamReader(responseBody);
            using var csvReader = new CsvReader(responseBodyReader, CultureInfo.InvariantCulture);

            csvReader.Read();
            csvReader.ReadHeader();
            csvReader.Context.HeaderRecord.Should().BeEquivalentTo(new[]
            {
                "LARS_QAN",
                "WHO_THIS_COURSE_IS_FOR",
                "ENTRY_REQUIREMENTS",
                "WHAT_YOU_WILL_LEARN",
                "HOW_YOU_WILL_LEARN",
                "WHAT_YOU_WILL_NEED_TO_BRING",
                "HOW_YOU_WILL_BE_ASSESSED",
                "WHERE_NEXT",
                "COURSE_NAME",
                "YOUR_REFERENCE",
                "DELIVERY_MODE",
                "START_DATE",
                "FLEXIBLE_START_DATE",
                "VENUE_NAME",
                "YOUR_VENUE_REFERENCE",
                "NATIONAL_DELIVERY",
                "SUB_REGION",
                "COURSE_WEBPAGE",
                "COST",
                "COST_DESCRIPTION",
                "DURATION",
                "DURATION_UNIT",
                "STUDY_MODE",
                "ATTENDANCE_PATTERN"
            });

            var rows = csvReader.GetRecords<CsvCourseRow>();
            rows.Should().HaveCount(1);
        }

        [Fact]
        public async Task Get_WithNoCourses_ReturnsEmptyFile()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerName: "Test Provider");
            Clock.UtcNow = new DateTime(2021, 4, 9, 13, 0, 0);

            // Act
            var response = await HttpClient.GetAsync($"/data-upload/courses/download?providerId={provider.ProviderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.FileName.Should().Be("\"Test Provider_courses_202104091300.csv\"");

            using var responseBody = await response.Content.ReadAsStreamAsync();
            using var responseBodyReader = new StreamReader(responseBody);
            using var csvReader = new CsvReader(responseBodyReader, CultureInfo.InvariantCulture);
            var rows = csvReader.GetRecords<CsvCourseRow>();
            rows.Should().HaveCount(0);
        }


        [Fact]
        public async Task Get_WorkbasedCourses_ReturnsCorrectResults()
        {
            // Arrange
            Clock.UtcNow = new DateTime(2021, 4, 9, 13, 0, 0);
            var provider = await TestData.CreateProvider(providerName: "Test Provider");
            var course = await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo(),
                configureCourseRuns: b => b.WithCourseRun(
                    CourseDeliveryMode.WorkBased,
                    null,
                    null,
                    venueId: null,
                    national: false,
                    courseUrl: "https://sometest.com",
                    cost: 61,
                    costDescription: "sixty one",
                    startDate:Clock.UtcNow, 
                    subRegionIds: new[] { "E06000001" }));
            var crun = course.CourseRuns.FirstOrDefault();

            // Act
            var response = await HttpClient.GetAsync($"/data-upload/courses/download?providerId={provider.ProviderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.FileName.Should().Be("\"Test Provider_courses_202104091300.csv\"");

            using var responseBody = await response.Content.ReadAsStreamAsync();
            using var responseBodyReader = new StreamReader(responseBody);
            using var csvReader = new CsvReader(responseBodyReader, CultureInfo.InvariantCulture);

            csvReader.Read();
            csvReader.ReadHeader();
            var rows = csvReader.GetRecords<CsvCourseRow>().ToArray();
            rows.Length.Should().Be(1);
            var downloadedcourse = rows.FirstOrDefault();

            downloadedcourse.Should().BeEquivalentTo(new CsvCourseRow()
            {
                LearnAimRef = course.LearnAimRef,
                WhoThisCourseIsFor = course.CourseDescription,
                EntryRequirements = course.EntryRequirements,
                WhatYouWillLearn = course.WhatYoullLearn,
                WhatYouWillNeedToBring = course.WhatYoullNeed,
                HowYouWillLearn = course.HowYoullLearn,
                HowYouWillBeAssessed = course.HowYoullBeAssessed,
                WhereNext = course.WhereNext,
                CourseName = crun.CourseName,
                ProviderCourseRef = crun.ProviderCourseID ?? "",
                DeliveryMode = "Work based",
                StartDate = Clock.UtcNow.ToString("dd/MM/yyyy"),
                FlexibleStartDate = "No",
                VenueName = "",
                ProviderVenueRef = "",
                NationalDelivery = "No",
                SubRegions = "County Durham",
                CourseWebPage = "https://sometest.com",
                Cost = 61.ToString("0.0000"),
                CostDescription = "sixty one",
                DurationUnit = "months",
                Duration = crun.DurationValue?.ToString(),
                StudyMode = "",
                AttendancePattern = "",
            });
        }

        [Fact]
        public async Task Get_Online_ReturnsCorrectResults()
        {
            // Arrange
            Clock.UtcNow = new DateTime(2021, 4, 9, 13, 0, 0);
            var provider = await TestData.CreateProvider(providerName: "Test Provider");
            var course = await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo(),
                configureCourseRuns: b => b.WithCourseRun(
                    CourseDeliveryMode.Online,
                    null,
                    null,
                    venueId: null,
                    national: true,
                    courseUrl: "https://someonlinecourse.com",
                    cost: 61,
                    costDescription: "sixty one",
                    startDate: Clock.UtcNow));
            var crun = course.CourseRuns.FirstOrDefault();

            // Act
            var response = await HttpClient.GetAsync($"/data-upload/courses/download?providerId={provider.ProviderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.FileName.Should().Be("\"Test Provider_courses_202104091300.csv\"");

            using var responseBody = await response.Content.ReadAsStreamAsync();
            using var responseBodyReader = new StreamReader(responseBody);
            using var csvReader = new CsvReader(responseBodyReader, CultureInfo.InvariantCulture);

            csvReader.Read();
            csvReader.ReadHeader();
            var rows = csvReader.GetRecords<CsvCourseRow>().ToArray();
            rows.Length.Should().Be(1);
            var downloadedcourse = rows.FirstOrDefault();

            downloadedcourse.Should().BeEquivalentTo(new CsvCourseRow()
            {
                LearnAimRef = course.LearnAimRef,
                WhoThisCourseIsFor = course.CourseDescription,
                EntryRequirements = course.EntryRequirements,
                WhatYouWillLearn = course.WhatYoullLearn,
                WhatYouWillNeedToBring = course.WhatYoullNeed,
                HowYouWillLearn = course.HowYoullLearn,
                HowYouWillBeAssessed = course.HowYoullBeAssessed,
                WhereNext = course.WhereNext,
                CourseName = crun.CourseName,
                ProviderCourseRef = crun.ProviderCourseID ?? "",
                DeliveryMode = "online",
                StartDate = Clock.UtcNow.ToString("dd/MM/yyyy"),
                FlexibleStartDate = "No",
                VenueName = "",
                ProviderVenueRef = "",
                NationalDelivery = "Yes",
                SubRegions = "",
                CourseWebPage = "https://someonlinecourse.com",
                Cost = 61.ToString("0.0000"),
                CostDescription = "sixty one",
                DurationUnit = "months",
                Duration = crun.DurationValue?.ToString(),
                StudyMode = "",
                AttendancePattern = "",
            });
        }

        [Fact]
        public async Task Get_ClassroomBased_ReturnsCorrectOrder()
        {
            // Arrange
            Clock.UtcNow = new DateTime(2021, 4, 9, 13, 0, 0);
            var provider = await TestData.CreateProvider(providerName: "Test Provider");
            var venue1 = await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), venueName: Faker.Company.Name(), providerVenueRef: "someRef");
            var venue2 = await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), venueName: Faker.Company.Name(), providerVenueRef: "someRef");
            var course = await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo(), learnAimRef:"AAAAAAAA",
                configureCourseRuns: b => b.WithCourseRun(
                    CourseDeliveryMode.ClassroomBased,
                    null,
                    null,
                    venueId: venue1.VenueId,
                    national: false,
                    courseUrl: "https://someclassroomcourse.com",
                    cost: 61,
                    costDescription: "sixty one",
                    startDate: Clock.UtcNow
                    ));
            var course1 = await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo(),
                configureCourseRuns: b => b.WithCourseRun(
                    CourseDeliveryMode.ClassroomBased,
                    null,
                    null,
                    venueId: venue2.VenueId,
                    national: false,
                    courseUrl: "https://someclassroomcourse.com",
                    cost: 61,
                    costDescription: "sixty one",
                    startDate: Clock.UtcNow));
            var crun = course.CourseRuns.FirstOrDefault();
            var crun1 = course1.CourseRuns.FirstOrDefault();


            // Act
            var response = await HttpClient.GetAsync($"/data-upload/courses/download?providerId={provider.ProviderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.FileName.Should().Be("\"Test Provider_courses_202104091300.csv\"");

            using var responseBody = await response.Content.ReadAsStreamAsync();
            using var responseBodyReader = new StreamReader(responseBody);
            using var csvReader = new CsvReader(responseBodyReader, CultureInfo.InvariantCulture);

            csvReader.Read();
            csvReader.ReadHeader();
            var rows = csvReader.GetRecords<CsvCourseRow>().ToArray();
            rows.Length.Should().Be(2);

            rows.Should().BeEquivalentTo(
                new CsvCourseRow()
                {
                    LearnAimRef = course1.LearnAimRef,
                    WhoThisCourseIsFor = course1.CourseDescription,
                    EntryRequirements = course1.EntryRequirements,
                    WhatYouWillLearn = course1.WhatYoullLearn,
                    WhatYouWillNeedToBring = course1.WhatYoullNeed,
                    HowYouWillLearn = course1.HowYoullLearn,
                    HowYouWillBeAssessed = course1.HowYoullBeAssessed,
                    WhereNext = course1.WhereNext,
                    CourseName = crun1.CourseName,
                    ProviderCourseRef = crun1.ProviderCourseID ?? "",
                    DeliveryMode = "classroom",
                    StartDate = Clock.UtcNow.ToString("dd/MM/yyyy"),
                    FlexibleStartDate = "No",
                    VenueName = venue2.VenueName,
                    ProviderVenueRef = "someRef",
                    NationalDelivery = "No",
                    SubRegions = "",
                    CourseWebPage = "https://someclassroomcourse.com",
                    Cost = 61.ToString("0.0000"),
                    CostDescription = "sixty one",
                    DurationUnit = "months",
                    Duration = crun1.DurationValue?.ToString(),
                    StudyMode = "",
                    AttendancePattern = "",
                },
                new CsvCourseRow()
                {
                    LearnAimRef = course.LearnAimRef,
                    WhoThisCourseIsFor = course.CourseDescription,
                    EntryRequirements = course.EntryRequirements,
                    WhatYouWillLearn = course.WhatYoullLearn,
                    WhatYouWillNeedToBring = course.WhatYoullNeed,
                    HowYouWillLearn = course.HowYoullLearn,
                    HowYouWillBeAssessed = course.HowYoullBeAssessed,
                    WhereNext = course.WhereNext,
                    CourseName = crun.CourseName,
                    ProviderCourseRef = crun.ProviderCourseID ?? "",
                    DeliveryMode = "classroom",
                    StartDate = Clock.UtcNow.ToString("dd/MM/yyyy"),
                    FlexibleStartDate = "No",
                    VenueName = venue1.VenueName,
                    ProviderVenueRef = "someRef",
                    NationalDelivery = "No",
                    SubRegions = "",
                    CourseWebPage = "https://someclassroomcourse.com",
                    Cost = 61.ToString("0.0000"),
                    CostDescription = "sixty one",
                    DurationUnit = "months",
                    Duration = crun.DurationValue?.ToString(),
                    StudyMode = "",
                    AttendancePattern = "",
                });
        }
    }
}

