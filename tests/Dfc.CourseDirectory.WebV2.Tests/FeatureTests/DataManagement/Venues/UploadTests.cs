using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
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

            var row1 = new VenueRow()
            {
                AddressLine1 = Faker.Address.StreetAddress(),
                Postcode = Faker.Address.UkPostCode(),
                VenueName = Faker.Company.Name()
            };

            var csvStream = CreateCsvStream(new[] { row1 });
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

            var row1 = new VenueRow()
            {
                AddressLine1 = Faker.Address.StreetAddress(),
                Postcode = Faker.Address.UkPostCode(),
                VenueName = Faker.Company.Name()
            };

            var csvStream = CreateCsvStream(new[] { row1 });
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

            var oldUpload = await TestData.CreateVenueUpload(provider.ProviderId, createdBy: User.ToUserInfo(), UploadStatus.Processed);

            var row1 = new VenueRow()
            {
                AddressLine1 = Faker.Address.StreetAddress(),
                Postcode = Faker.Address.UkPostCode(),
                VenueName = Faker.Company.Name()
            };

            var csvStream = CreateCsvStream(new[] { row1 });
            var requestContent = CreateMultiPartDataContent("text/csv", csvStream);

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/venues/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.EnsureNonErrorStatusCode();

            oldUpload = await WithSqlQueryDispatcher(
                dispatcher => dispatcher.ExecuteQuery(new GetVenueUpload() { VenueUploadId = oldUpload.VenueUploadId }));
            oldUpload.UploadStatus.Should().Be(UploadStatus.Abandoned);
        }

        [Fact]
        public async Task Post_ValidVenuesFileProcessingCompletedWithinThreshold_CreatesRecordAndRedirectsToCheckAndPublish()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var row1 = new VenueRow()
            {
                AddressLine1 = Faker.Address.StreetAddress(),
                Postcode = Faker.Address.UkPostCode(),
                VenueName = Faker.Company.Name()
            };

            var csvStream = CreateCsvStream(new[] { row1 });
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
                        dispatcher => dispatcher.ExecuteQuery(new UpdateVenueUploadStatus()
                        {
                            VenueUploadId = venueUploadId,
                            ChangedOn = Clock.UtcNow,
                            UploadStatus = UploadStatus.Processed
                        })).GetAwaiter().GetResult();
                });

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/venues/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.Should().Be($"/data-upload/venues/check-publish?providerId={provider.ProviderId}");
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

        [Theory]
        [InlineData("")]
        [InlineData("77u/")]  // UTF-8 BOM
        public async Task Post_EmptyFile_RendersError(string base64Content)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var csvStream = new MemoryStream(Convert.FromBase64String(base64Content));
            var requestContent = CreateMultiPartDataContent("text/csv", csvStream);

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/venues/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("File", "The selected file is empty");
        }

        private MultipartFormDataContent CreateMultiPartDataContent(string contentType, MemoryStream stream)
        {
            var content = new MultipartFormDataContent();
            content.Headers.ContentType.MediaType = "multipart/form-data";

            using (var mem = new MemoryStream())
            using (var writer = new StreamWriter(mem))
            {
                if (stream != null)
                {
                    var byteArrayContent = new ByteArrayContent(stream.ToArray());
                    byteArrayContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                    content.Add(byteArrayContent, "File", "someFileName.csv");
                }
            }
            return content;
        }

        private MemoryStream CreateCsvStream(IEnumerable<VenueRow> rows)
        {
            using (var mem = new MemoryStream())
            using (var writer = new StreamWriter(mem))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteHeader<VenueRow>();
                csvWriter.WriteRecords(rows);
                csvWriter.Flush();
                return mem;
            }
        }
    }
}
