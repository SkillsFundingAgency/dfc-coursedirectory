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
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Apprenticeships
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
        public async Task Post_MissingFile_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/apprenticeships/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("File", "Select a CSV");
        }

        [Fact]
        public async Task Post_InvalidFile_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            // This data is a small PNG file
            var nonCsvContent = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAABcAAAAbCAIAAAAYioOMAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAHYcAAB2HAY/l8WUAAAEkSURBVEhLY/hPDTBqCnYwgkz5tf/ge0sHIPqxai1UCDfAacp7Q8u3MipA9E5ZGyoEA7+vXPva0IJsB05TgJohpgARVOj//++LlgF1wsWBCGIHTlO+TZkBVwoV6Z0IF0FGQCkUU36fPf8pJgkeEMjqcBkBVA+URZjy99UruC+ADgGKwJV+a++GsyEIGGpfK2t/HTsB0YswBRhgcEUQ38K5yAhrrCFMgUcKBAGdhswFIjyxjjAFTc87LSMUrrL2n9t3oUoxAE5T0BAkpHABqCmY7kdGn5MzIcpwAagpyEGLiSBq8AAGzOQIQT937IKzoWpxAwa4UmQESUtwLkQpHgA1BS0VQQBppgBt/vfjB1QACZBmClYjgIA0UwgiqFrcgBqm/P8PAGN09WCiWJ70AAAAAElFTkSuQmCC");

            var fileStream = new MemoryStream(nonCsvContent);
            var requestContent = CreateMultiPartDataContent("text/csv", fileStream);

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/apprenticeships/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("File", "The selected file must be a CSV");
        }

        [Fact]
        public async Task Post_EmptyFile_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var csvStream = new MemoryStream(Convert.FromBase64String(""));
            var requestContent = CreateMultiPartDataContent("text/csv", csvStream);

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/apprenticeships/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("File", "The selected file is empty");
        }

        /// <summary>
        /// TODO: Test that needs to be wired up as part of validate story.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Post_FileIsTooLarge_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var csvStream = new MemoryStream(new byte[5242880 + 1]);
            var requestContent = CreateMultiPartDataContent("text/csv", csvStream);

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/apprenticeships/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("File", "The selected file must be smaller than 5MB");
        }

        [Fact]
        public async Task Get_Returns_Ok()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/apprenticeships?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
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

        [Fact]
        public async Task Post_FileHasMissingHeaders_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var csvStream = DataManagementFileHelper.CreateCsvStream(
                csvWriter =>
                {
                    // Miss out STANDARD_CODE, VENUE
                    csvWriter.WriteField("STANDARD_VERSION");
                    csvWriter.WriteField("APPRENTICESHIP_INFORMATION");
                    csvWriter.WriteField("APPRENTICESHIP_WEBPAGE");
                    csvWriter.WriteField("CONTACT_EMAIL");
                    csvWriter.WriteField("CONTACT_PHONE");
                    csvWriter.WriteField("CONTACT_URL");
                    csvWriter.WriteField("DELIVERY_METHOD");
                    csvWriter.WriteField("YOUR_VENUE_REFERENCE");
                    csvWriter.WriteField("RADIUS");
                    csvWriter.WriteField("DELIVERY_MODE");
                    csvWriter.WriteField("NATIONAL_DELIVERY");
                    csvWriter.WriteField("SUB_REGION");
                    csvWriter.NextRecord();
                });

            var requestContent = CreateMultiPartDataContent("text/csv", csvStream);

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/apprenticeships/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("File", "Enter headings in the correct format");
            doc.GetAllElementsByTestId("MissingHeader").Select(e => e.TextContent.Trim()).Should().BeEquivalentTo(new[]
            {
                "STANDARD_CODE",
                "VENUE"
            });
        }
    }
}
