using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Courses
{
    public class DeleteRowGroupTests : MvcTestBase
    {
        public DeleteRowGroupTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_RowGroupDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = await TestData.CreateLearningAimRef();

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.IsValid = false;
                        record.Errors = new[] { "COURSE_WHO_THIS_COURSE_IS_FOR_REQUIRED" };
                        record.WhoThisCourseIsFor = string.Empty;
                    });
                });

            var rowNumber = courseUploadRows.Last().RowNumber + 1;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/data-upload/courses/resolve/{rowNumber}/course/delete?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_ValidRequest_ReturnsExpectedContent()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = await TestData.CreateLearningAimRef();

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    var courseId = Guid.NewGuid();

                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.IsValid = false;
                        record.Errors = new[] { "COURSE_WHO_THIS_COURSE_IS_FOR_REQUIRED" };
                        record.WhoThisCourseIsFor = string.Empty;
                        record.CourseId = courseId;
                        record.DeliveryMode = "online";
                        record.ResolvedDeliveryMode = CourseDeliveryMode.Online;
                    });

                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.CourseId = courseId;
                        record.DeliveryMode = "classroom";
                        record.ResolvedDeliveryMode = CourseDeliveryMode.ClassroomBased;
                    });
                });

            var rowNumber = courseUploadRows.First().RowNumber;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/data-upload/courses/resolve/{rowNumber}/course/delete?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("DeliveryModes").TextContent.Should().Be("classroom based and online");
        }

        [Fact]
        public async Task Get_ARowHasUnresolvedDeliveryMode_RendersAllDeliveryModesLabel()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = await TestData.CreateLearningAimRef();

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.IsValid = false;
                        record.Errors = new[] { "COURSE_WHO_THIS_COURSE_IS_FOR_REQUIRED" };
                        record.WhoThisCourseIsFor = string.Empty;
                        record.DeliveryMode = "";
                        record.ResolvedDeliveryMode = null;
                    });
                });

            var rowNumber = courseUploadRows.First().RowNumber;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/data-upload/courses/resolve/{rowNumber}/course/delete?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("DeliveryModes").TextContent.Should().Be("classroom based, online and work based");
        }

        [Fact]
        public async Task Post_RowGroupDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = await TestData.CreateLearningAimRef();

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.IsValid = false;
                        record.Errors = new[] { "COURSE_WHO_THIS_COURSE_IS_FOR_REQUIRED" };
                        record.WhoThisCourseIsFor = string.Empty;
                    });
                });

            var rowNumber = courseUploadRows.Last().RowNumber + 1;

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/data-upload/courses/resolve/{rowNumber}/course/delete?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_NotConfirmed_ReturnsError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = await TestData.CreateLearningAimRef();

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.IsValid = false;
                        record.Errors = new[] { "COURSE_WHO_THIS_COURSE_IS_FOR_REQUIRED" };
                        record.WhoThisCourseIsFor = string.Empty;
                    });
                });

            var rowNumber = courseUploadRows.First().RowNumber;

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/data-upload/courses/resolve/{rowNumber}/course/delete?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("Confirm", "Confirm you want to delete the course");
        }

        [Theory]
        [InlineData(true, "/data-upload/courses/resolve?providerId={0}")]
        [InlineData(false, "/data-upload/courses/check-publish?providerId={0}")]
        public async Task Post_ValidRequest_DeletesRowAndRedirects(bool otherRowsHaveErrors, string expectedLocation)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = await TestData.CreateLearningAimRef();

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    // Configure 3 rows - 2 with the same course ID (that we are deleting)
                    // and one more with a different course ID.
                    // Both records with the same course ID should be deleted, the other should not be.

                    var courseId1 = Guid.NewGuid();

                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.CourseId = courseId1;
                        record.IsValid = false;
                        record.Errors = new[] { "COURSE_WHO_THIS_COURSE_IS_FOR_REQUIRED" };
                        record.WhoThisCourseIsFor = string.Empty;
                    });

                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.CourseId = courseId1;
                        record.IsValid = false;
                        record.Errors = new[] { "COURSE_WHO_THIS_COURSE_IS_FOR_REQUIRED" };
                        record.WhoThisCourseIsFor = string.Empty;
                    });

                    var courseId2 = Guid.NewGuid();

                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.CourseId = courseId2;

                        if (otherRowsHaveErrors)
                        {
                            record.IsValid = false;
                            record.Errors = new[] { "COURSE_WHO_THIS_COURSE_IS_FOR_REQUIRED" };
                            record.WhoThisCourseIsFor = string.Empty;
                        }
                    });
                });

            var rowNumber = courseUploadRows.First().RowNumber;

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/data-upload/courses/resolve/{rowNumber}/course/delete?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("Confirm", "true")
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should().Be(string.Format(expectedLocation, provider.ProviderId));

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                var (rows, _) = await dispatcher.ExecuteQuery(new GetCourseUploadRows() { CourseUploadId = courseUpload.CourseUploadId });

                // We expect rows 2 and 3 to be deleted (since they have the same CourseId) and 4 to be still live
                rows.Count.Should().Be(1);
                rows.Last().RowNumber.Should().Be(4);
            });
        }
    }
}
