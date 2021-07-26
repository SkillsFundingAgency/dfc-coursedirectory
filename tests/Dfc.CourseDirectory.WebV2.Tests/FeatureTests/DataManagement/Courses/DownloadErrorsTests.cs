using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Courses
{
    public class DownloadErrorsTests : MvcTestBase
    {
        public DownloadErrorsTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(null)]
        [InlineData(UploadStatus.Created)]
        [InlineData(UploadStatus.Processing)]
        [InlineData(UploadStatus.ProcessedSuccessfully)]
        [InlineData(UploadStatus.Published)]
        [InlineData(UploadStatus.Abandoned)]
        public async Task Get_ProviderHasNoCourseUploadAtProcessedWithErrorsStatus_ReturnsError(UploadStatus? uploadStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            if (uploadStatus != null)
            {
                await TestData.CreateCourseUpload(provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus.Value);
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/courses/download-errors?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_ValidRequest_ReturnsRowsWithErrors()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerName: "Test Provider");
            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;
            Clock.UtcNow = new DateTime(2021, 4, 9, 13, 0, 0);

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.CourseName = string.Empty;
                        record.VenueName = string.Empty;
                        record.Errors = new[]
                        {
                            ErrorRegistry.All["COURSERUN_COURSE_NAME_REQUIRED"].ErrorCode,
                            ErrorRegistry.All["COURSERUN_VENUE_REQUIRED"].ErrorCode
                        };
                    });
                });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/courses/download-errors?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.FileName.Should().Be("\"Test Provider_courses_errors_202104091300.csv\"");

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
                "ATTENDANCE_PATTERN",
                "ERRORS"
            });

            var rows = csvReader.GetRecords<CsvCourseRowWithErrors>();
            rows.Should().BeEquivalentTo(
                new CsvCourseRowWithErrors()
                {
                    LearnAimRef = courseUploadRows[0].LearnAimRef,
                    WhoThisCourseIsFor = courseUploadRows[0].WhoThisCourseIsFor,
                    EntryRequirements = courseUploadRows[0].EntryRequirements,
                    WhatYouWillLearn = courseUploadRows[0].WhatYouWillLearn,
                    HowYouWillLearn = courseUploadRows[0].HowYouWillLearn,
                    WhatYouWillNeedToBring = courseUploadRows[0].WhatYouWillNeedToBring,
                    HowYouWillBeAssessed = courseUploadRows[0].HowYouWillBeAssessed,
                    WhereNext = courseUploadRows[0].WhereNext,
                    CourseName = courseUploadRows[0].CourseName,
                    ProviderCourseRef = courseUploadRows[0].ProviderCourseRef,
                    DeliveryMode = courseUploadRows[0].DeliveryMode,
                    StartDate = courseUploadRows[0].StartDate,
                    FlexibleStartDate = courseUploadRows[0].FlexibleStartDate,
                    VenueName = courseUploadRows[0].VenueName,
                    ProviderVenueRef = courseUploadRows[0].ProviderVenueRef,
                    NationalDelivery = courseUploadRows[0].NationalDelivery,
                    SubRegions = courseUploadRows[0].SubRegions,
                    CourseWebPage = courseUploadRows[0].CourseWebPage,
                    Cost = courseUploadRows[0].Cost,
                    CostDescription = courseUploadRows[0].CostDescription,
                    Duration = courseUploadRows[0].Duration,
                    DurationUnit = courseUploadRows[0].DurationUnit,
                    StudyMode = courseUploadRows[0].StudyMode,
                    AttendancePattern = courseUploadRows[0].AttendancePattern,
                    Errors = ErrorRegistry.All["COURSERUN_COURSE_NAME_REQUIRED"].GetMessage() + "\n"
                        + ErrorRegistry.All["COURSERUN_VENUE_REQUIRED"].GetMessage()
                });
        }
    }
}
