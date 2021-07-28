using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Courses
{
    public class ErrorsTests : MvcTestBase
    {
        public ErrorsTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(null)]
        [InlineData(UploadStatus.Published)]
        [InlineData(UploadStatus.Abandoned)]
        public async Task Get_NoUnpublishedCourseUpload_ReturnsBadRequest(UploadStatus? uploadStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            if (uploadStatus != null)
            {
                await TestData.CreateCourseUpload(provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus.Value);
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/courses/errors?providerId={provider.ProviderId}");

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

            var (_, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef: learnAimRef, record =>
                    {
                        record.CourseName = string.Empty;
                        record.Cost = null;
                        record.CostDescription = null;
                        record.DeliveryMode = "Classroom Based";
                        record.StartDate = "09/01/2021"; 
                        record.IsValid = false;
                        record.Errors = new[]
                        {
                            ErrorRegistry.All["COURSERUN_COURSE_NAME_REQUIRED"].ErrorCode,
                            ErrorRegistry.All["COURSERUN_COST_REQUIRED"].ErrorCode
                        };
                    });
                });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/courses/errors?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                // Should only have one error row in the table
                var errorRows = doc.GetAllElementsByTestId("CourseRunRow");

                doc.GetElementByTestId("ErrorCount").TextContent.Should().Be("2");

                errorRows.Count().Should().Be(1);

                var errors = errorRows.Single().GetElementByTestId("Errors").GetTrimmedTextContent();
                errors.Should().BeEquivalentTo(
                    Core.DataManagement.Errors.MapCourseErrorToFieldGroup("COURSERUN_COURSE_NAME_REQUIRED") + ", " +
                    Core.DataManagement.Errors.MapCourseErrorToFieldGroup("COURSERUN_COST_REQUIRED")
                );

                doc.GetElementByTestId("ResolveOnScreenOption").Should().NotBeNull();
            }
        }

        [Fact]
        public async Task Get_MoreThan30RowsWithErrors_DoesNotRender_ResolveOnScreenOption()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var (_, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    for (int i = 0; i < 31; i++)
                    {
                        rowBuilder.AddRow(learnAimRef: learnAimRef, record =>
                        {
                            record.CourseName = string.Empty;
                            record.IsValid = false;
                            record.Errors = new[]
                            {
                                ErrorRegistry.All["COURSERUN_COURSE_NAME_REQUIRED"].ErrorCode,
                            };
                        });
                    }
                });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/courses/errors?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("ResolveOnScreenOption").Should().BeNull();
        }

        [Fact]
        public async Task Post_MissingWhatNext_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var (_, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef: learnAimRef, record =>
                    {
                        record.CourseName = string.Empty;
                        record.IsValid = false;
                        record.Errors = new[]
                        {
                            ErrorRegistry.All["COURSERUN_COURSE_NAME_REQUIRED"].ErrorCode,
                        };
                    });
                });

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/courses/errors?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("WhatNext", "Select what you want to do");
        }

        [Theory]
        [InlineData("UploadNewFile", "/data-upload/courses?providerId={0}")]
        [InlineData("DeleteUpload", "/data-upload/courses/delete?providerId={0}")]
        public async Task Post_ValidRequest_ReturnsRedirect(string selectedOption, string expectedLocation)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var (_, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef: learnAimRef, record =>
                    {
                        record.CourseName = string.Empty;
                        record.IsValid = false;
                        record.Errors = new[]
                        {
                            ErrorRegistry.All["COURSERUN_COURSE_NAME_REQUIRED"].ErrorCode,
                        };
                    });
                });

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/courses/errors?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("WhatNext", selectedOption)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should().Be(string.Format(expectedLocation, provider.ProviderId));
        }
    }
}
