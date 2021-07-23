using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Courses
{
    public class ResolveRowDetailsTests : MvcTestBase
    {
        public ResolveRowDetailsTests(CourseDirectoryApplicationFactory factory)
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
        public async Task Get_NoVenueUploadAtProcessedWithErrorsStatus_ReturnsBadRequest(UploadStatus? uploadStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: User.ToUserInfo());

            if (uploadStatus.HasValue)
            {
                var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                    provider.ProviderId,
                    createdBy: User.ToUserInfo(),
                    uploadStatus.Value);
            }

            var rowNumber = 2;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/data-upload/courses/resolve/{rowNumber}/details?providerId={provider.ProviderId}&deliveryMode=ClassroomBased");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_RowDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: User.ToUserInfo());

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddValidRow(learnAimRef);
                });

            var rowNumber = courseUploadRows.First().RowNumber + 1;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/data-upload/courses/resolve/{rowNumber}/details?providerId={provider.ProviderId}&deliveryMode=ClassroomBased");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_RowDoesNotHaveErrors_ReturnsBadRequest()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: User.ToUserInfo());

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddValidRow(learnAimRef);
                });

            var rowNumber = courseUploadRows.First().RowNumber;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/data-upload/courses/resolve/{rowNumber}/details?providerId={provider.ProviderId}&deliveryMode=ClassroomBased");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_ValidRequest_RendersExpectedContent()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: User.ToUserInfo());

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.CourseName = string.Empty;
                        record.IsValid = false;
                        record.Errors = new[] { "COURSERUN_COURSE_NAME_REQUIRED" };
                    });
                });

            var rowNumber = courseUploadRows.First().RowNumber;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/data-upload/courses/resolve/{rowNumber}/details?providerId={provider.ProviderId}&deliveryMode=ClassroomBased");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.AssertHasError("CourseName", ErrorRegistry.All["COURSERUN_COURSE_NAME_REQUIRED"].GetMessage());
        }

        [Theory]
        [InlineData(null)]
        [InlineData(UploadStatus.Created)]
        [InlineData(UploadStatus.Processing)]
        [InlineData(UploadStatus.ProcessedSuccessfully)]
        [InlineData(UploadStatus.Published)]
        [InlineData(UploadStatus.Abandoned)]
        public async Task Post_NoVenueUploadAtProcessedWithErrorsStatus_ReturnsBadRequest(UploadStatus? uploadStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            if (uploadStatus.HasValue)
            {
                var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                    provider.ProviderId,
                    createdBy: User.ToUserInfo(),
                    uploadStatus.Value);
            }

            var rowNumber = 2;

            var courseName = "Course name";
            var providerCourseRef = "REF";
            var flexibleStartDate = "true";
            var courseWebPage = "provider.com/course";
            var cost = "42.00";
            var duration = "3";
            var durationUnit = "Months";

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/data-upload/courses/resolve/{rowNumber}/details?providerId={provider.ProviderId}&deliveryMode=Online")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("CourseName", courseName)
                    .Add("ProviderCourseRef", providerCourseRef)
                    .Add("FlexibleStartDate", flexibleStartDate)
                    .Add("CourseWebPage", courseWebPage)
                    .Add("Cost", cost)
                    .Add("Duration", duration)
                    .Add("DurationUnit", durationUnit)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Post_RowDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddValidRow(learnAimRef);
                });

            var rowNumber = courseUploadRows.First().RowNumber + 1;

            var courseName = "Course name";
            var providerCourseRef = "REF";
            var flexibleStartDate = "true";
            var courseWebPage = "provider.com/course";
            var cost = "42.00";
            var duration = "3";
            var durationUnit = "Months";

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/data-upload/courses/resolve/{rowNumber}/details?providerId={provider.ProviderId}&deliveryMode=Online")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("CourseName", courseName)
                    .Add("ProviderCourseRef", providerCourseRef)
                    .Add("FlexibleStartDate", flexibleStartDate)
                    .Add("CourseWebPage", courseWebPage)
                    .Add("Cost", cost)
                    .Add("Duration", duration)
                    .Add("DurationUnit", durationUnit)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_RowDoesNotHaveErrors_ReturnsBadRequest()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddValidRow(learnAimRef);
                });

            var rowNumber = courseUploadRows.First().RowNumber;

            var courseName = "Course name";
            var providerCourseRef = "REF";
            var flexibleStartDate = "true";
            var courseWebPage = "provider.com/course";
            var cost = "42.00";
            var duration = "3";
            var durationUnit = "Months";

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/data-upload/courses/resolve/{rowNumber}/details?providerId={provider.ProviderId}&deliveryMode=Online")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("CourseName", courseName)
                    .Add("ProviderCourseRef", providerCourseRef)
                    .Add("FlexibleStartDate", flexibleStartDate)
                    .Add("CourseWebPage", courseWebPage)
                    .Add("Cost", cost)
                    .Add("Duration", duration)
                    .Add("DurationUnit", durationUnit)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [MemberData(nameof(GetInvalidFixData))]
        public async Task Post_WithErrors_RendersExpectedError(
            string deliveryMode,
            string courseName,
            string providerCourseRef,
            string flexibleStartDate,
            string startDateDay,
            string startDateMonth,
            string startDateYear,
            string nationalDelivery,
            IEnumerable<string> subRegionIds,
            string courseWebPage,
            string cost,
            string costDescription,
            string duration,
            string durationUnit,
            string studyMode,
            string attendancePattern,
            bool specifyVenueId,
            string expectedErrorInputId,
            string expectedErrorCode)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: User.ToUserInfo());

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.IsValid = false;
                        record.Errors = new[] { "COURSERUN_COURSE_NAME_REQUIRED" };
                        record.CourseName = string.Empty;
                    });
                });

            var rowNumber = courseUploadRows.First().RowNumber;

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/data-upload/courses/resolve/{rowNumber}/details?providerId={provider.ProviderId}&deliveryMode={deliveryMode}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("CourseName", courseName)
                    .Add("ProviderCourseRef", providerCourseRef)
                    .Add("FlexibleStartDate", flexibleStartDate)
                    .Add("StartDate-Day", startDateDay)
                    .Add("StartDate-Month", startDateMonth)
                    .Add("StartDate-Year", startDateYear)
                    .Add("NationalDelivery", nationalDelivery)
                    .Add("SubRegionIds", subRegionIds)
                    .Add("CourseWebPage", courseWebPage)
                    .Add("Cost", cost)
                    .Add("CostDescription", costDescription)
                    .Add("Duration", duration)
                    .Add("DurationUnit", durationUnit)
                    .Add("StudyMode", studyMode)
                    .Add("AttendancePattern", attendancePattern)
                    .Add("VenueId", specifyVenueId ? venue.VenueId.ToString() : null)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasErrorWithCode(expectedErrorInputId, expectedErrorCode);
        }

        [Fact]
        public async Task Post_ValidRequestForClassroomBasedDeliveryMode_UpdatesRowCorrectly()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: User.ToUserInfo());

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.DeliveryMode = "classroom";
                        record.ResolvedDeliveryMode = CourseDeliveryMode.ClassroomBased;
                        record.StartDate = "01/02/2020";
                        record.ResolvedStartDate = new DateTime(2020, 2, 1);
                        record.FlexibleStartDate = "no";
                        record.ResolvedFlexibleStartDate = false;
                        record.Cost = string.Empty;
                        record.CostDescription = "The cost";

                        // Set fields that aren't allowed for ClassroomBased so we can check they're cleared out correctly
                        record.NationalDelivery = "yes";
                        record.ResolvedNationalDelivery = true;
                        record.SubRegions = "County Durham";
                        record.ResolvedSubRegions = new[] { "E06000001" };

                        record.IsValid = false;
                        record.Errors = new[]
                        {
                            "COURSERUN_NATIONAL_DELIVERY_NOT_ALLOWED",
                            "COURSERUN_SUBREGIONS_NOT_ALLOWED",
                            "COURSERUN_VENUE_REQUIRED"
                        };
                    });
                });

            var rowNumber = courseUploadRows.First().RowNumber;

            var courseName = "Course name";
            var providerCourseRef = "REF";
            var flexibleStartDate = "true";
            var courseWebPage = "provider.com/course";
            var cost = "42.00";
            var duration = "3";
            var durationUnit = "Months";
            var studyMode = "flexible";
            var attendancePattern = "evening";

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/data-upload/courses/resolve/{rowNumber}/details?providerId={provider.ProviderId}&deliveryMode=ClassroomBased")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("CourseName", courseName)
                    .Add("ProviderCourseRef", providerCourseRef)
                    .Add("FlexibleStartDate", flexibleStartDate)
                    .Add("CourseWebPage", courseWebPage)
                    .Add("Cost", cost)
                    .Add("Duration", duration)
                    .Add("DurationUnit", durationUnit)
                    .Add("StudyMode", studyMode)
                    .Add("AttendancePattern", attendancePattern)
                    .Add("VenueId", venue.VenueId)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureNonErrorStatusCode();

            var row = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new GetCourseUploadRowDetail()
            {
                CourseUploadId = courseUpload.CourseUploadId,
                RowNumber = rowNumber
            }));

            using (new AssertionScope())
            {
                row.IsValid.Should().BeTrue();
                row.Errors.Should().BeEmpty();
                row.DeliveryMode.Should().BeEquivalentTo("classroom based");
                row.ResolvedDeliveryMode.Should().Be(CourseDeliveryMode.ClassroomBased);
                row.CourseName.Should().Be(courseName);
                row.ProviderCourseRef.Should().Be(providerCourseRef);
                row.StartDate.Should().BeNullOrEmpty();
                row.ResolvedStartDate.Should().BeNull();
                row.FlexibleStartDate.Should().BeEquivalentTo("yes");
                row.ResolvedFlexibleStartDate.Should().BeTrue();
                row.NationalDelivery.Should().BeNullOrEmpty();
                row.ResolvedNationalDelivery.Should().BeNull();
                row.SubRegions.Should().BeNullOrEmpty();
                row.ResolvedSubRegionIds.Should().BeEmpty();
                row.CourseWebPage.Should().Be(courseWebPage);
                row.Cost.Should().Be(cost);
                row.ResolvedCost.Should().Be(42.00m);
                row.CostDescription.Should().BeNullOrEmpty();
                row.Duration.Should().Be(duration);
                row.ResolvedDuration.Should().Be(3);
                row.DurationUnit.Should().BeEquivalentTo(durationUnit);
                row.ResolvedDurationUnit.Should().Be(CourseDurationUnit.Months);
                row.StudyMode.Should().BeEquivalentTo(studyMode);
                row.ResolvedStudyMode.Should().Be(CourseStudyMode.Flexible);
                row.AttendancePattern.Should().BeEquivalentTo(attendancePattern);
                row.ResolvedAttendancePattern.Should().Be(CourseAttendancePattern.Evening);
                row.VenueId.Should().Be(venue.VenueId);
                row.VenueName.Should().Be(venue.VenueName);
                row.ProviderVenueRef.Should().Be(venue.ProviderVenueRef);
            }
        }

        [Fact]
        public async Task Post_ValidRequestForOnlineDeliveryMode_UpdatesRowCorrectly()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: User.ToUserInfo());

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.DeliveryMode = "online";
                        record.ResolvedDeliveryMode = CourseDeliveryMode.Online;
                        record.StartDate = "01/02/2020";
                        record.ResolvedStartDate = new DateTime(2020, 2, 1);
                        record.FlexibleStartDate = "no";
                        record.ResolvedFlexibleStartDate = false;
                        record.Cost = string.Empty;
                        record.CostDescription = "The cost";

                        // Set fields that aren't allowed for Online so we can check they're cleared out correctly
                        record.NationalDelivery = "yes";
                        record.ResolvedNationalDelivery = true;
                        record.SubRegions = "County Durham";
                        record.ResolvedSubRegions = new[] { "E06000001" };
                        record.StudyMode = "full time";
                        record.ResolvedStudyMode = CourseStudyMode.FullTime;
                        record.AttendancePattern = "daytime";
                        record.ResolvedAttendancePattern = CourseAttendancePattern.Daytime;
                        record.VenueId = venue.VenueId;
                        record.VenueName = venue.VenueName;
                        record.ProviderVenueRef = venue.ProviderVenueRef;

                        record.IsValid = false;
                        record.Errors = new[]
                        {
                            "COURSERUN_NATIONAL_DELIVERY_NOT_ALLOWED",
                            "COURSERUN_SUBREGIONS_NOT_ALLOWED",
                            "COURSERUN_STUDY_MODE_NOT_ALLOWED",
                            "COURSERUN_ATTENDANCE_PATTERN_NOT_ALLOWED",
                            "COURSERUN_PROVIDER_VENUE_REF_NOT_ALLOWED"
                        };
                    });
                });

            var rowNumber = courseUploadRows.First().RowNumber;

            var courseName = "Course name";
            var providerCourseRef = "REF";
            var flexibleStartDate = "true";
            var courseWebPage = "provider.com/course";
            var cost = "42.00";
            var duration = "3";
            var durationUnit = "Months";

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/data-upload/courses/resolve/{rowNumber}/details?providerId={provider.ProviderId}&deliveryMode=Online")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("CourseName", courseName)
                    .Add("ProviderCourseRef", providerCourseRef)
                    .Add("FlexibleStartDate", flexibleStartDate)
                    .Add("CourseWebPage", courseWebPage)
                    .Add("Cost", cost)
                    .Add("Duration", duration)
                    .Add("DurationUnit", durationUnit)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureNonErrorStatusCode();

            var row = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new GetCourseUploadRowDetail()
            {
                CourseUploadId = courseUpload.CourseUploadId,
                RowNumber = rowNumber
            }));

            using (new AssertionScope())
            {
                row.IsValid.Should().BeTrue();
                row.Errors.Should().BeEmpty();
                row.DeliveryMode.Should().BeEquivalentTo("online");
                row.ResolvedDeliveryMode.Should().Be(CourseDeliveryMode.Online);
                row.CourseName.Should().Be(courseName);
                row.ProviderCourseRef.Should().Be(providerCourseRef);
                row.StartDate.Should().BeNullOrEmpty();
                row.ResolvedStartDate.Should().BeNull();
                row.FlexibleStartDate.Should().BeEquivalentTo("yes");
                row.ResolvedFlexibleStartDate.Should().BeTrue();
                row.NationalDelivery.Should().BeNullOrEmpty();
                row.ResolvedNationalDelivery.Should().BeNull();
                row.SubRegions.Should().BeNullOrEmpty();
                row.ResolvedSubRegionIds.Should().BeEmpty();
                row.CourseWebPage.Should().Be(courseWebPage);
                row.Cost.Should().Be(cost);
                row.ResolvedCost.Should().Be(42.00m);
                row.CostDescription.Should().BeNullOrEmpty();
                row.Duration.Should().Be(duration);
                row.ResolvedDuration.Should().Be(3);
                row.DurationUnit.Should().BeEquivalentTo(durationUnit);
                row.ResolvedDurationUnit.Should().Be(CourseDurationUnit.Months);
                row.StudyMode.Should().BeNullOrEmpty();
                row.ResolvedStudyMode.Should().BeNull();
                row.AttendancePattern.Should().BeNullOrEmpty();
                row.ResolvedAttendancePattern.Should().BeNull();
                row.VenueId.Should().BeNull();
                row.VenueName.Should().BeNullOrEmpty();
                row.ProviderVenueRef.Should().BeNullOrEmpty();
            }
        }

        [Fact]
        public async Task Post_ValidRequestForWorkBasedDeliveryMode_UpdatesRowCorrectly()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: User.ToUserInfo());

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.DeliveryMode = "work based";
                        record.ResolvedDeliveryMode = CourseDeliveryMode.WorkBased;
                        record.StartDate = "01/02/2020";
                        record.ResolvedStartDate = new DateTime(2020, 2, 1);
                        record.FlexibleStartDate = "no";
                        record.ResolvedFlexibleStartDate = false;
                        record.Cost = string.Empty;
                        record.CostDescription = "The cost";
                        record.NationalDelivery = "yes";
                        record.ResolvedNationalDelivery = true;

                        // Set fields that aren't allowed for Online so we can check they're cleared out correctly
                        record.StudyMode = "full time";
                        record.ResolvedStudyMode = CourseStudyMode.FullTime;
                        record.AttendancePattern = "daytime";
                        record.ResolvedAttendancePattern = CourseAttendancePattern.Daytime;
                        record.VenueId = venue.VenueId;
                        record.VenueName = venue.VenueName;
                        record.ProviderVenueRef = venue.ProviderVenueRef;

                        record.IsValid = false;
                        record.Errors = new[]
                        {
                            "COURSERUN_STUDY_MODE_NOT_ALLOWED",
                            "COURSERUN_ATTENDANCE_PATTERN_NOT_ALLOWED",
                            "COURSERUN_PROVIDER_VENUE_REF_NOT_ALLOWED"
                        };
                    });
                });

            var rowNumber = courseUploadRows.First().RowNumber;

            var courseName = "Course name";
            var providerCourseRef = "REF";
            var flexibleStartDate = "true";
            var courseWebPage = "provider.com/course";
            var cost = "42.00";
            var duration = "3";
            var durationUnit = "Months";
            var nationalDelivery = "false";
            var subRegionIds = new[] { "E06000001", "E06000002" };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/data-upload/courses/resolve/{rowNumber}/details?providerId={provider.ProviderId}&deliveryMode=WorkBased")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("CourseName", courseName)
                    .Add("ProviderCourseRef", providerCourseRef)
                    .Add("FlexibleStartDate", flexibleStartDate)
                    .Add("NationalDelivery", nationalDelivery)
                    .Add("SubRegionIds", subRegionIds)
                    .Add("CourseWebPage", courseWebPage)
                    .Add("Cost", cost)
                    .Add("Duration", duration)
                    .Add("DurationUnit", durationUnit)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureNonErrorStatusCode();

            var row = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new GetCourseUploadRowDetail()
            {
                CourseUploadId = courseUpload.CourseUploadId,
                RowNumber = rowNumber
            }));

            using (new AssertionScope())
            {
                row.IsValid.Should().BeTrue();
                row.Errors.Should().BeEmpty();
                row.DeliveryMode.Should().BeEquivalentTo("work based");
                row.ResolvedDeliveryMode.Should().Be(CourseDeliveryMode.WorkBased);
                row.CourseName.Should().Be(courseName);
                row.ProviderCourseRef.Should().Be(providerCourseRef);
                row.StartDate.Should().BeNullOrEmpty();
                row.ResolvedStartDate.Should().BeNull();
                row.FlexibleStartDate.Should().BeEquivalentTo("yes");
                row.ResolvedFlexibleStartDate.Should().BeTrue();
                row.NationalDelivery.Should().BeEquivalentTo("no");
                row.ResolvedNationalDelivery.Should().BeFalse();
                row.SubRegions.Should().BeEquivalentTo("County Durham; Darlington");
                row.ResolvedSubRegionIds.Should().BeEquivalentTo(subRegionIds);
                row.CourseWebPage.Should().Be(courseWebPage);
                row.Cost.Should().Be(cost);
                row.ResolvedCost.Should().Be(42.00m);
                row.CostDescription.Should().BeNullOrEmpty();
                row.Duration.Should().Be(duration);
                row.ResolvedDuration.Should().Be(3);
                row.DurationUnit.Should().BeEquivalentTo(durationUnit);
                row.ResolvedDurationUnit.Should().Be(CourseDurationUnit.Months);
                row.StudyMode.Should().BeNullOrEmpty();
                row.ResolvedStudyMode.Should().BeNull();
                row.AttendancePattern.Should().BeNullOrEmpty();
                row.ResolvedAttendancePattern.Should().BeNull();
                row.VenueId.Should().BeNull();
                row.VenueName.Should().BeNullOrEmpty();
                row.ProviderVenueRef.Should().BeNullOrEmpty();
            }
        }

        [Theory]
        [InlineData(true, "/data-upload/courses/resolve?providerId={0}")]
        [InlineData(false, "/data-upload/courses/check-publish?providerId={0}")]
        public async Task Post_ValidRequest_UpdatesRowsAndRedirects(bool otherRowsHaveErrors, string expectedLocation)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.IsValid = false;
                        record.Errors = new[] { "COURSERUN_COURSE_NAME_REQUIRED" };
                        record.CourseName = string.Empty;
                    });

                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        if (otherRowsHaveErrors)
                        {
                            record.IsValid = false;
                            record.Errors = new[] { "COURSERUN_COURSE_NAME_REQUIRED" };
                            record.CourseName = string.Empty;
                        }
                    });
                });

            var rowNumber = courseUploadRows.First().RowNumber;

            var courseName = "Course name";
            var providerCourseRef = "REF";
            var flexibleStartDate = "true";
            var courseWebPage = "provider.com/course";
            var cost = "42.00";
            var duration = "3";
            var durationUnit = "Months";

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/data-upload/courses/resolve/{rowNumber}/details?providerId={provider.ProviderId}&deliveryMode=Online")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("CourseName", courseName)
                    .Add("ProviderCourseRef", providerCourseRef)
                    .Add("FlexibleStartDate", flexibleStartDate)
                    .Add("CourseWebPage", courseWebPage)
                    .Add("Cost", cost)
                    .Add("Duration", duration)
                    .Add("DurationUnit", durationUnit)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should().Be(string.Format(expectedLocation, provider.ProviderId));
        }

        public static IEnumerable<object[]> GetInvalidFixData()
        {
            yield return CreateRecord(
                expectedErrorInputId: "CourseName",
                expectedErrorCode: "COURSERUN_COURSE_NAME_REQUIRED",
                courseName: string.Empty);

            yield return CreateRecord(
                expectedErrorInputId: "CourseName",
                expectedErrorCode: "COURSERUN_COURSE_NAME_FORMAT",
                courseName: "😁");

            yield return CreateRecord(
                expectedErrorInputId: "CourseName",
                expectedErrorCode: "COURSERUN_COURSE_NAME_MAXLENGTH",
                courseName: new string('x', 256));

            yield return CreateRecord(
                expectedErrorInputId: "ProviderCourseRef",
                expectedErrorCode: "COURSERUN_PROVIDER_COURSE_REF_FORMAT",
                providerCourseRef: "😁");

            yield return CreateRecord(
                expectedErrorInputId: "ProviderCourseRef",
                expectedErrorCode: "COURSERUN_PROVIDER_COURSE_REF_MAXLENGTH",
                providerCourseRef: new string('x', 256));

            yield return CreateRecord(
                expectedErrorInputId: "FlexibleStartDate",
                expectedErrorCode: "COURSERUN_FLEXIBLE_START_DATE_REQUIRED",
                flexibleStartDate: null);

            yield return CreateRecord(
                expectedErrorInputId: "StartDate",
                expectedErrorCode: "COURSERUN_START_DATE_REQUIRED",
                flexibleStartDate: "false",
                startDateDay: null,
                startDateMonth: null,
                startDateYear: null);

            yield return CreateRecord(
                expectedErrorInputId: "NationalDelivery",
                expectedErrorCode: "COURSERUN_NATIONAL_DELIVERY_REQUIRED",
                deliveryMode: "WorkBased",
                nationalDelivery: null);

            // SubRegions are weird since they don't have a single field with an error attached
            // TODO figure out how to check for the error..
            //yield return CreateRecord(
            //    expectedErrorInputId: "SubRegionIds",
            //    expectedErrorCode: "COURSERUN_SUBREGIONS_REQUIRED",
            //    deliveryMode: "WorkBased",
            //    nationalDelivery: "false",
            //    subRegionIds: null);

            yield return CreateRecord(
                expectedErrorInputId: "CourseWebPage",
                expectedErrorCode: "COURSERUN_COURSE_WEB_PAGE_FORMAT",
                courseWebPage: "notarealwebsite");
            
            yield return CreateRecord(
                expectedErrorInputId: "CourseWebPage",
                expectedErrorCode: "COURSERUN_COURSE_WEB_PAGE_MAXLENGTH",
                courseWebPage: "www." + new string('x', 256) + ".com");

            yield return CreateRecord(
                expectedErrorInputId: "Cost",
                expectedErrorCode: "COURSERUN_COST_INVALID",
                cost: "x");

            yield return CreateRecord(
                expectedErrorInputId: "Cost",
                expectedErrorCode: "COURSERUN_COST_REQUIRED",
                cost: null);

            yield return CreateRecord(
                expectedErrorInputId: "CostDescription",
                expectedErrorCode: "COURSERUN_COST_DESCRIPTION_MAXLENGTH",
                costDescription: new string('x', 256));

            yield return CreateRecord(
                expectedErrorInputId: "Duration",
                expectedErrorCode: "COURSERUN_DURATION_REQUIRED",
                duration: null);

            yield return CreateRecord(
                expectedErrorInputId: "Duration",
                expectedErrorCode: "COURSERUN_DURATION_RANGE",
                duration: "-1");

            yield return CreateRecord(
                expectedErrorInputId: "DurationUnit",
                expectedErrorCode: "COURSERUN_DURATION_UNIT_REQUIRED",
                durationUnit: null);

            yield return CreateRecord(
                expectedErrorInputId: "StudyMode",
                expectedErrorCode: "COURSERUN_STUDY_MODE_REQUIRED",
                deliveryMode: "ClassroomBased",
                studyMode: null);

            yield return CreateRecord(
                expectedErrorInputId: "AttendancePattern",
                expectedErrorCode: "COURSERUN_ATTENDANCE_PATTERN_REQUIRED",
                deliveryMode: "ClassroomBased",
                attendancePattern: null);

            yield return CreateRecord(
                expectedErrorInputId: "VenueId",
                expectedErrorCode: "COURSERUN_VENUE_REQUIRED",
                deliveryMode: "ClassroomBased",
                specifyVenueId: false);

            static object[] CreateRecord(
                    string expectedErrorInputId,
                    string expectedErrorCode,
                    string deliveryMode = "Online",
                    string courseName = "Course name",
                    string providerCourseRef = "REF",
                    string flexibleStartDate = "true",
                    string startDateDay = "",
                    string startDateMonth = "",
                    string startDateYear = "",
                    string nationalDelivery = "yes",
                    IEnumerable<string> subRegionIds = null,
                    string courseWebPage = "provider.com/course",
                    string cost = "42.00",
                    string costDescription = "",
                    string duration = "3",
                    string durationUnit = "Months",
                    string studyMode = null,
                    string attendancePattern = null,
                    bool? specifyVenueId = null) => new object[]
                {
                    deliveryMode,
                    courseName,
                    providerCourseRef,
                    flexibleStartDate,
                    startDateDay,
                    startDateMonth,
                    startDateYear,
                    nationalDelivery,
                    subRegionIds,
                    courseWebPage,
                    cost,
                    costDescription,
                    duration,
                    durationUnit,
                    studyMode,
                    attendancePattern,
                    specifyVenueId ?? deliveryMode == "ClassroomBased",
                    expectedErrorInputId,
                    expectedErrorCode
                };
        }
    }
}
