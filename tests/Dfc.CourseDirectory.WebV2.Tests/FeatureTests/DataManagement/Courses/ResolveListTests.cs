using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Courses
{
    public class ResolveListTests : MvcTestBase
    {
        public ResolveListTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(null)]
        [InlineData(UploadStatus.Created)]
        [InlineData(UploadStatus.Processing)]
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

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/Courses/resolve?providerId={provider.ProviderId}");

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

            var (_, CourseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    rowBuilder.AddValidRows(learnAimRef: string.Empty, 2);

                    rowBuilder.AddRow(learnAimRef: string.Empty, record =>
                    {
                        record.CourseName = string.Empty;
                        record.DeliveryMode = "Classroom Based";
                        record.StartDate = "09/01/2021";
                        record.IsValid = false;
                        record.Errors = new[]
                        {
                            ErrorRegistry.All["COURSERUN_COURSE_NAME_REQUIRED"].ErrorCode,
                        };
                    });
                });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/Courses/resolve?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                // Should only have one error row in the table
                var errorRows = doc.GetAllElementsByTestId("CourseRunRow");

                errorRows.Count().Should().Be(1);

                var firstRowCells = errorRows.Single().GetElementByTestId("Errors").TextContent.Trim();
                firstRowCells.Should().BeEquivalentTo(
                    Core.DataManagement.Errors.MapCourseErrorToFieldGroup("COURSERUN_COURSE_NAME_REQUIRED")
                );
            }
        }

        [Fact]
        public async Task Get_RowHasDescriptionErrors_RendersResolveAndDeleteButtons()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (_, CourseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef: string.Empty, record =>
                    {
                        record.WhoThisCourseIsFor = string.Empty;
                        record.IsValid = false;
                        record.Errors = new[]
                        {
                            ErrorRegistry.All["COURSE_WHO_THIS_COURSE_IS_FOR_REQUIRED"].ErrorCode,
                        };
                    });
                });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/Courses/resolve?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                doc.GetElementByTestId("ResolveDescription").Should().NotBeNull();
                doc.GetElementByTestId("DeleteDescription").Should().NotBeNull();
            }
        }

        [Fact]
        public async Task Get_RowDoesNotHaveDescriptionErrors_DoesNotRenderResolveAndDeleteButtons()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (_, CourseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef: string.Empty, record =>
                    {
                        record.CourseName = string.Empty;
                        record.IsValid = false;
                        record.Errors = new[]
                        {
                            ErrorRegistry.All["COURSERUN_COURSE_NAME_REQUIRED"].ErrorCode,
                        };
                    });
                });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/Courses/resolve?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                doc.GetElementByTestId("ResolveDescription").Should().BeNull();
                doc.GetElementByTestId("DeleteDescription").Should().BeNull();
            }
        }

        [Fact]
        public async Task Get_CourseDescriptionErrorsOnly_DoesNotRenderDetailsRow()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (_, CourseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef: string.Empty, record =>
                    {
                        record.WhoThisCourseIsFor = string.Empty;
                        record.IsValid = false;
                        record.Errors = new[]
                        {
                            ErrorRegistry.All["COURSE_WHO_THIS_COURSE_IS_FOR_REQUIRED"].ErrorCode,
                        };
                    });
                });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/Courses/resolve?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetAllElementsByTestId("CourseRunRow").Count.Should().Be(0);
        }

        [Fact]
        public async Task Get_RowHasDeliveryModeError_RendersResolveAndDeleteLinks()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (_, CourseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef: string.Empty, record =>
                    {
                        record.DeliveryMode = string.Empty;
                        record.IsValid = false;
                        record.Errors = new[]
                        {
                            ErrorRegistry.All["COURSERUN_DELIVERY_MODE_REQUIRED"].ErrorCode,
                        };
                    });
                });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/Courses/resolve?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                doc.GetElementByTestId("ResolveDelivery").Should().NotBeNull();
                doc.GetElementByTestId("ResolveDetails").Should().BeNull();
                doc.GetElementByTestId("DeleteDetails").Should().NotBeNull();
            }
        }

        [Fact]
        public async Task Get_RowHasDetailsError_RendersResolveAndDeleteLinks()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (_, CourseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef: string.Empty, record =>
                    {
                        record.CourseName = string.Empty;
                        record.IsValid = false;
                        record.Errors = new[]
                        {
                            ErrorRegistry.All["COURSERUN_COURSE_NAME_REQUIRED"].ErrorCode,
                        };
                    });
                });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/Courses/resolve?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                doc.GetElementByTestId("ResolveDelivery").Should().BeNull();
                doc.GetElementByTestId("ResolveDetails").Should().NotBeNull();
                doc.GetElementByTestId("DeleteDetails").Should().NotBeNull();
            }
        }
    }
}
