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
    }
}
