using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using Moq;
using OneOf.Types;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Courses
{
    public class UploadTests : MvcTestBase
    {
        public UploadTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
            DataUploadsContainerClient = new Mock<BlobContainerClient>();

            DataUploadsContainerClient
                .Setup(mock => mock.CreateIfNotExistsAsync(
                    It.IsAny<PublicAccessType>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateMockResponse(BlobsModelFactory.BlobContainerInfo(new ETag(), Clock.UtcNow)));

            BlobServiceClient
                .Setup(mock => mock.GetBlobContainerClient("data-uploads"))
                .Returns(DataUploadsContainerClient.Object);

            static Response<T> CreateMockResponse<T>(T value)
            {
                var mock = new Mock<Response<T>>();
                mock.SetupGet(mock => mock.Value).Returns(value);
                return mock.Object;
            }
        }

        private Mock<BlobContainerClient> DataUploadsContainerClient { get; }

        [Fact]
        public async Task Get_HasLiveVenues_DoesRenderDownloadLink()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = await TestData.CreateLearningAimRef();
            await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo(), learnAimRef: learnAimRef);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/courses?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("DownloadLink").Should().NotBeNull();
        }

        [Fact]
        public async Task Get_NoLiveVenues_DoesNotRenderDownloadLink()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/courses?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("DownloadLink").Should().BeNull();
        }

        [Fact]
        public async Task Post_ProviderAlreadyHasUnprocessedUpload_ReturnsBadRequest()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.FE);

            await TestData.CreateCourseUpload(provider.ProviderId, createdBy: User.ToUserInfo(), createdOn: Clock.UtcNow);

            var csvStream = DataManagementFileHelper.CreateVenueUploadCsvStream(rowCount: 1);
            var requestContent = CreateMultiPartDataContent("text/csv", csvStream);

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/courses/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Post_ValidCoursesFile_CreatesRecordAndRedirectsToInProgress()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var learnAimRef = await TestData.CreateLearningAimRef();

            var csvStream = DataManagementFileHelper.CreateCourseUploadCsvStream(learnAimRef, rowCount: 1);
            var requestContent = CreateMultiPartDataContent("text/csv", csvStream);

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/courses/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.Should().Be($"/data-upload/courses/in-progress?providerId={provider.ProviderId}");

            SqlQuerySpy.VerifyQuery<CreateCourseUpload, Success>(q =>
                q.CreatedBy.UserId == User.UserId &&
                q.CreatedOn == Clock.UtcNow &&
                q.ProviderId == provider.ProviderId);
        }

        [Fact]
        public async Task Post_ValidCoursesFile_AbandonsExistingUnpublishedUpload()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.FE);
            var learnAimRef = await TestData.CreateLearningAimRef();

            var (oldUpload, _) = await TestData.CreateCourseUpload(provider.ProviderId, createdBy: User.ToUserInfo(), UploadStatus.ProcessedSuccessfully);

            var csvStream = DataManagementFileHelper.CreateCourseUploadCsvStream(learnAimRef, rowCount: 1);
            var requestContent = CreateMultiPartDataContent("text/csv", csvStream);

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/courses/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.EnsureNonErrorStatusCode();

            oldUpload = await WithSqlQueryDispatcher(
                dispatcher => dispatcher.ExecuteQuery(new GetCourseUpload() { CourseUploadId = oldUpload.CourseUploadId }));
            oldUpload.UploadStatus.Should().Be(UploadStatus.Abandoned);
        }

        [Fact]
        public async Task Post_MissingFile_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.FE);

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/courses/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("File", "Select a CSV");
        }

        [Fact]
        public async Task Post_InvalidFile_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.FE);

            // This data is a small PNG file
            var nonCsvContent = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAABcAAAAbCAIAAAAYioOMAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAHYcAAB2HAY/l8WUAAAEkSURBVEhLY/hPDTBqCnYwgkz5tf/ge0sHIPqxai1UCDfAacp7Q8u3MipA9E5ZGyoEA7+vXPva0IJsB05TgJohpgARVOj//++LlgF1wsWBCGIHTlO+TZkBVwoV6Z0IF0FGQCkUU36fPf8pJgkeEMjqcBkBVA+URZjy99UruC+ADgGKwJV+a++GsyEIGGpfK2t/HTsB0YswBRhgcEUQ38K5yAhrrCFMgUcKBAGdhswFIjyxjjAFTc87LSMUrrL2n9t3oUoxAE5T0BAkpHABqCmY7kdGn5MzIcpwAagpyEGLiSBq8AAGzOQIQT937IKzoWpxAwa4UmQESUtwLkQpHgA1BS0VQQBppgBt/vfjB1QACZBmClYjgIA0UwgiqFrcgBqm/P8PAGN09WCiWJ70AAAAAElFTkSuQmCC");

            var fileStream = new MemoryStream(nonCsvContent);
            var requestContent = CreateMultiPartDataContent("text/csv", fileStream);

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/courses/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("File", "The selected file must be a CSV");
        }

        [Fact]
        public async Task Post_EmptyFile_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.FE);

            var csvStream = new MemoryStream(Convert.FromBase64String(""));
            var requestContent = CreateMultiPartDataContent("text/csv", csvStream);

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/courses/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("File", "The selected file is empty");
        }

        [Fact]
        public async Task Post_FileHasMissingHeaders_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var csvStream = DataManagementFileHelper.CreateCsvStream(
                csvWriter =>
                {
                    // Miss out WHO_THIS_COURSE_IS_FOR, YOUR_REFERENCE
                    csvWriter.WriteField("LARS_QAN");
                    csvWriter.WriteField("ENTRY_REQUIREMENTS");
                    csvWriter.WriteField("WHAT_YOU_WILL_LEARN");
                    csvWriter.WriteField("HOW_YOU_WILL_LEARN");
                    csvWriter.WriteField("WHAT_YOU_WILL_NEED_TO_BRING");
                    csvWriter.WriteField("HOW_YOU_WILL_BE_ASSESSED");
                    csvWriter.WriteField("WHERE_NEXT");
                    csvWriter.WriteField("COURSE_NAME");
                    csvWriter.WriteField("DELIVERY_MODE");
                    csvWriter.WriteField("START_DATE");
                    csvWriter.WriteField("FLEXIBLE_START_DATE");
                    csvWriter.WriteField("VENUE_NAME");
                    csvWriter.WriteField("YOUR_VENUE_REFERENCE");
                    csvWriter.WriteField("NATIONAL_DELIVERY");
                    csvWriter.WriteField("SUB_REGION");
                    csvWriter.WriteField("COURSE_WEBPAGE");
                    csvWriter.WriteField("COST");
                    csvWriter.WriteField("COST_DESCRIPTION");
                    csvWriter.WriteField("DURATION");
                    csvWriter.WriteField("DURATION_UNIT");
                    csvWriter.WriteField("STUDY_MODE");
                    csvWriter.WriteField("ATTENDANCE_PATTERN");
                    csvWriter.NextRecord();
                });

            var requestContent = CreateMultiPartDataContent("text/csv", csvStream);

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/courses/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("File", "Enter headings in the correct format");
            doc.GetAllElementsByTestId("MissingHeader").Select(e => e.TextContent.Trim()).Should().BeEquivalentTo(new[]
            {
                "WHO_THIS_COURSE_IS_FOR",
                "YOUR_REFERENCE"
            });
        }

        [Fact]
        public async Task Post_FileIsTooLarge_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.FE);

            var csvStream = new MemoryStream(new byte[5242880 + 1]);
            var requestContent = CreateMultiPartDataContent("text/csv", csvStream);

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/courses/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("File", "The selected file must be smaller than 5MB");
        }

        private MultipartFormDataContent CreateMultiPartDataContent(string contentType, Stream csvStream)
        {
            var content = new MultipartFormDataContent();
            content.Headers.ContentType.MediaType = "multipart/form-data";

            using (var mem = new MemoryStream())
            using (var writer = new StreamWriter(mem))
            {
                if (csvStream != null)
                {
                    var byteArrayContent = new StreamContent(csvStream);
                    byteArrayContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                    content.Add(byteArrayContent, "File", "someFileName.csv");
                }
            }
            return content;
        }

            //doc.GetAllElementsByTestId("MissingLars").Select(e => e.TextContent.Trim()).Should().BeEquivalentTo(new[]
           // {
           //     "Row 3",
           //     "Row 5"
           // });
        


        [Fact]
        public async Task Post_FileWithLarsErrors_RendersExpectedResult()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var expiredLearningAimRef = await TestData.CreateLearningAimRef(DateTime.Today.AddDays(-1));
            var validLearningAimRef = await TestData.CreateLearningAimRef();

            //Add missing lars
            var learningAimRef = await TestData.CreateLearningAimRef();
            List<CsvCourseRow> courseUploadRows = DataManagementFileHelper.CreateCourseUploadRows(validLearningAimRef, 1).ToList();
            courseUploadRows.AddRange(DataManagementFileHelper.CreateCourseUploadRows("", 1).ToList());
            courseUploadRows.AddRange(DataManagementFileHelper.CreateCourseUploadRows(validLearningAimRef, 1).ToList());
            courseUploadRows.AddRange(DataManagementFileHelper.CreateCourseUploadRows("    ", 1).ToList());

            //Add invalid and expired lars
            courseUploadRows.AddRange(DataManagementFileHelper.CreateCourseUploadRows("ABCDEFG", 1).ToList());
            courseUploadRows.AddRange(DataManagementFileHelper.CreateCourseUploadRows(expiredLearningAimRef, 1).ToList());
            courseUploadRows.AddRange(DataManagementFileHelper.CreateCourseUploadRows("GFEDCBA", 1).ToList());

            var stream = DataManagementFileHelper.CreateCourseUploadCsvStream(courseUploadRows.ToArray());

            var requestContent = CreateMultiPartDataContent("text/csv", stream);

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/courses/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("File", "The file contains errors and could not be uploaded");
            doc.GetAllElementsByTestId("MissingLars").Select(e => e.TextContent.Trim()).Should().BeEquivalentTo(new[]
            {
                "Row 3",
                "Row 5"
            });
            doc.GetAllElementsByTestId("InvalidLars").Select(e => e.TextContent.Trim()).Should().BeEquivalentTo(new[]
            {
                "Row 6",
                "Row 8"
            });
            doc.GetAllElementsByTestId("ExpiredLars").Select(e => e.TextContent.Trim()).Should().BeEquivalentTo(new[]
            {
                string.Format("Row {0}, expired code {1}", 7, expiredLearningAimRef)
            });
        }
    }
}
