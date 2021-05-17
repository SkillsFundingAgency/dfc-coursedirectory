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
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using Moq;
using OneOf.Types;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Venues
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
        public async Task Get_RendersPage()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            // Act
            var response = await HttpClient.GetAsync($"/data-upload/venues?providerId={provider.ProviderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Post_ProviderAlreadyHasUnprocessedUpload_ReturnsBadRequest()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new CreateVenueUpload()
            {
                CreatedBy = User.ToUserInfo(),
                CreatedOn = Clock.UtcNow,
                ProviderId = provider.ProviderId,
                VenueUploadId = Guid.NewGuid()
            }));

            var csvStream = DataManagementFileHelper.CreateVenueUploadCsvStream(rowCount: 1);
            var requestContent = CreateMultiPartDataContent("text/csv", csvStream);

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/venues/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Post_ValidVenuesFile_CreatesRecordAndRedirectsToInProgress()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var csvStream = DataManagementFileHelper.CreateVenueUploadCsvStream(rowCount: 1);
            var requestContent = CreateMultiPartDataContent("text/csv", csvStream);

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/venues/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.Should().Be($"/data-upload/venues/in-progress?providerId={provider.ProviderId}");

            SqlQuerySpy.VerifyQuery<CreateVenueUpload, Success>(q =>
                q.CreatedBy.UserId == User.UserId &&
                q.CreatedOn == Clock.UtcNow &&
                q.ProviderId == provider.ProviderId);
        }

        [Fact]
        public async Task Post_ValidUpload_AbandonsExistingUnpublishedUpload()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (oldUpload, _) = await TestData.CreateVenueUpload(provider.ProviderId, createdBy: User.ToUserInfo(), UploadStatus.ProcessedSuccessfully);

            var csvStream = DataManagementFileHelper.CreateVenueUploadCsvStream(rowCount: 1);
            var requestContent = CreateMultiPartDataContent("text/csv", csvStream);

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/venues/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.EnsureNonErrorStatusCode();

            oldUpload = await WithSqlQueryDispatcher(
                dispatcher => dispatcher.ExecuteQuery(new GetVenueUpload() { VenueUploadId = oldUpload.VenueUploadId }));
            oldUpload.UploadStatus.Should().Be(UploadStatus.Abandoned);
        }

        [Theory]
        [InlineData(true, "check-publish")]
        [InlineData(false, "errors")]
        public async Task Post_ValidVenuesFileProcessingCompletedWithinThreshold_CreatesRecordAndRedirectsToCheckAndPublish(
            bool processedSuccessfully,
            string expectedRedirectPath)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var csvStream = DataManagementFileHelper.CreateVenueUploadCsvStream(rowCount: 1);
            var requestContent = CreateMultiPartDataContent("text/csv", csvStream);

            // We need to hook into the SaveVenueFile method on IFileUploadProcessor so that we can update the
            // VenueUpload's status to be Processed before the WaitForVenueProcessingToComplete method is called.
            // We use SqlQuerySpy.Callback to capture the ID from the Create call then wait for the Blob Storage
            // upload to run. At that point we know the VenueUpload has been commited to the DB and we can update it.

            Guid venueUploadId = default;

            SqlQuerySpy.Callback<CreateVenueUpload, Success>(q => venueUploadId = q.VenueUploadId);

            DataUploadsContainerClient
                .Setup(mock => mock.UploadBlobAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback(() =>
                {
                    WithSqlQueryDispatcher(
                        dispatcher => dispatcher.ExecuteQuery(new SetVenueUploadProcessed()
                        {
                            VenueUploadId = venueUploadId,
                            IsValid = processedSuccessfully,
                            ProcessingCompletedOn = Clock.UtcNow
                        })).GetAwaiter().GetResult();
                });

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/venues/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.Should().Be($"/data-upload/venues/{expectedRedirectPath}?providerId={provider.ProviderId}");
        }

        [Fact]
        public async Task Post_MissingFile_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/venues/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("File", "Select a CSV");
        }

        [Fact]
        public async Task Post_InvalidFile_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            // This data is a small PNG file
            var nonCsvContent = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAABcAAAAbCAIAAAAYioOMAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAHYcAAB2HAY/l8WUAAAEkSURBVEhLY/hPDTBqCnYwgkz5tf/ge0sHIPqxai1UCDfAacp7Q8u3MipA9E5ZGyoEA7+vXPva0IJsB05TgJohpgARVOj//++LlgF1wsWBCGIHTlO+TZkBVwoV6Z0IF0FGQCkUU36fPf8pJgkeEMjqcBkBVA+URZjy99UruC+ADgGKwJV+a++GsyEIGGpfK2t/HTsB0YswBRhgcEUQ38K5yAhrrCFMgUcKBAGdhswFIjyxjjAFTc87LSMUrrL2n9t3oUoxAE5T0BAkpHABqCmY7kdGn5MzIcpwAagpyEGLiSBq8AAGzOQIQT937IKzoWpxAwa4UmQESUtwLkQpHgA1BS0VQQBppgBt/vfjB1QACZBmClYjgIA0UwgiqFrcgBqm/P8PAGN09WCiWJ70AAAAAElFTkSuQmCC");

            var fileStream = new MemoryStream(nonCsvContent);
            var requestContent = CreateMultiPartDataContent("text/csv", fileStream);

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/venues/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("File", "The selected file must be a CSV");
        }

        [Fact]
        public async Task Post_EmptyFile_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var csvStream = new MemoryStream(Convert.FromBase64String(""));
            var requestContent = CreateMultiPartDataContent("text/csv", csvStream);

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/venues/upload?providerId={provider.ProviderId}", requestContent);

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

            var csvStream = DataManagementFileHelper.CreateVenueUploadCsvStream(
                csvWriter =>
                {
                    // Miss out VENUE_NAME, POSTCODE
                    csvWriter.WriteField("YOUR_VENUE_REFERENCE");
                    csvWriter.WriteField("ADDRESS_LINE_1");
                    csvWriter.WriteField("ADDRESS_LINE_2");
                    csvWriter.WriteField("TOWN_OR_CITY");
                    csvWriter.WriteField("COUNTY");
                    csvWriter.WriteField("EMAIL");
                    csvWriter.WriteField("PHONE");
                    csvWriter.WriteField("WEBSITE");
                    csvWriter.NextRecord();
                },
                writeHeader: false);

            var requestContent = CreateMultiPartDataContent("text/csv", csvStream);
            
            // Act
            var response = await HttpClient.PostAsync($"/data-upload/venues/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("File", "Enter headings in the correct format");
            doc.GetAllElementsByTestId("MissingHeader").Select(e => e.TextContent.Trim()).Should().BeEquivalentTo(new[]
            {
                "VENUE_NAME",
                "POSTCODE"
            });
        }

        [Fact]
        public async Task Post_FileHasRowsWithInvalidColumns_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var csvStream = DataManagementFileHelper.CreateVenueUploadCsvStream(
                csvWriter =>
                {
                    csvWriter.WriteField("One column");
                    csvWriter.NextRecord();
                },
                writeHeader: true);

            var requestContent = CreateMultiPartDataContent("text/csv", csvStream);

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/venues/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("File", "The selected file must use the template");
        }

        [Fact]
        public async Task Post_FileIsTooLarge_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var csvStream = new MemoryStream(new byte[1048576 + 1]);
            var requestContent = CreateMultiPartDataContent("text/csv", csvStream);

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/venues/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("File", "The selected file must be smaller than 1MB");
        }

        //[Fact]
        //public async Task UnpublishedVenueCountDiplayedOnDashboard()
        //{
        //    // Arrange
        //    var provider = await TestData.CreateProvider();

        //    var csvStream = DataManagementFileHelper.CreateVenueUploadCsvStream(rowCount: 4);
        //    var requestContent = CreateMultiPartDataContent("text/csv", csvStream);

        //    var request = new HttpRequestMessage(HttpMethod.Get, $"/dashboard");

        //    // Act
        //    var response = await HttpClient.SendAsync(request);

        //    // Assert
        //    response.StatusCode.Should().Be(HttpStatusCode.OK);



        //    SqlQuerySpy.VerifyQuery<CreateVenueUpload, Success>(q =>
        //        q.CreatedBy.UserId == User.UserId &&
        //        q.CreatedOn == Clock.UtcNow &&
        //        q.ProviderId == provider.ProviderId);


        //    var doc = await response.GetDocument();
        //    doc.Body.TextContent.Should().Contain("You have no venues");
        //}

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
    }
}
