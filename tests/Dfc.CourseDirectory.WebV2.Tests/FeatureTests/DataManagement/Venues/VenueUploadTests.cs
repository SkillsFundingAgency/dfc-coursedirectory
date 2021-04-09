using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Venues
{
    public class UploadTests : MvcTestBase
    {
        public UploadTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Post_ValidVenuesFile_RedirectsToPublish()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var csvStream = CreateCsvStream(Enumerable.Empty<VenueRow>());
            var requestContent = CreateMultiPartDataContent("text/csv", csvStream);

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/venues/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.Should().Be($"/data-upload/venues/check-publish?providerId={provider.ProviderId}");
        }

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
        public async Task Post_MissingFile_RedirectsToValidation()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/venues/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.Should().Be($"/data-upload/venues/validation?providerId={provider.ProviderId}");
        }

        [Theory]
        [InlineData("application/vnd.ms-excel")]
        [InlineData("application/json")]
        [InlineData("text/plain")]
        public async Task Post_DataManagement_UnsupportedContentTypeFileRedirectsToValidationError(string contentType)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var csvStream = CreateCsvStream(Enumerable.Empty<VenueRow>());
            var requestContent = CreateMultiPartDataContent(contentType, csvStream);

            // Act
            var response = await HttpClient.PostAsync($"/data-upload/venues/upload?providerId={provider.ProviderId}", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.Should().Be($"/data-upload/venues/validation?providerId={provider.ProviderId}");
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
