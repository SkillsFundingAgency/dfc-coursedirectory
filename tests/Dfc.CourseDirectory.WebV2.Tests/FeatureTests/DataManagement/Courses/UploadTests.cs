using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using Moq;
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
        public async Task Get_RendersPage()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            // Act
            var response = await HttpClient.GetAsync($"/data-upload/courses?providerId={provider.ProviderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }


        [Fact]
        public async Task Post_MissingFile_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

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
            var provider = await TestData.CreateProvider();

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
            var provider = await TestData.CreateProvider();

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
        public async Task Post_FileIsTooLarge_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var csvStream = new MemoryStream(new byte[3145728 + 1]);
            var requestContent = CreateMultiPartDataContent("text/csv", csvStream);

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/courses/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("File", "The selected file must be smaller than 3MB");
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
    }
}
