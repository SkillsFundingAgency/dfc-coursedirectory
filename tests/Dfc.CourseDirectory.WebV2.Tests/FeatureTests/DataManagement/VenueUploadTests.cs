using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement
{
    public class VenueUploadTests : MvcTestBase
    {
        private string csvContentType => "text/csv";
        public VenueUploadTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        public async Task Post_DataManagement_ValidVenuesFileRedirectsToPublish()
        {
            // Arrange
            HttpResponseMessage response;
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);
            var csvStream = CreateCsvStream(new List<CsvVenue>());
            var requestContent = CreateMultiPartDataContent(csvContentType, csvStream);

            // Act
            response = await HttpClient.PostAsync("/data-upload/upload", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.Should().Be("/data-upload/venues/checkandpublish");
        }

        [Fact]
        public async Task Get_DataManagement_RendersPage()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);

            // Act
            var response = await HttpClient.GetAsync("/data-upload/venues");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Post_DataManagemen_MissingFileRedirectsToValidation()
        {
            // Arrange
            HttpResponseMessage response;
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);
            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            response = await HttpClient.PostAsync("/data-upload/upload", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.Should().Be("/data-upload/venues/validation");
        }

        [Theory]
        [InlineData("application/vnd.ms-excel")]
        [InlineData("application/json")]
        [InlineData("text/plain")]
        public async Task Post_DataManagement_UnsupportedContentTypeFileRedirectsToValidationError(string contentType)
        {
            // Arrange
            HttpResponseMessage response;
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);
            var csvStream = CreateCsvStream(new List<CsvVenue>());
            var requestContent = CreateMultiPartDataContent(contentType, csvStream);

            // Act
            response = await HttpClient.PostAsync("/data-upload/upload", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.Should().Be("/data-upload/venues/validation");
        }

        private MultipartFormDataContent CreateMultiPartDataContent(string contentType, MemoryStream stream)
        {
            var content = new MultipartFormDataContent("dfdfd");
            content.Headers.ContentType.MediaType = "multipart/form-data";
            using (var mem = new MemoryStream())
            using (var writer = new StreamWriter(mem))
            {
                if (stream != null)
                {
                    var byteArrayContent = new ByteArrayContent(stream.ToArray());
                    byteArrayContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                    content.Add(byteArrayContent, "Some Description", "someFileName.csv");
                }
            }
            return content;
        }

        private MemoryStream CreateCsvStream(List<CsvVenue> rows)
        {
            using (var mem = new MemoryStream())
            using (var writer = new StreamWriter(mem))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteHeader<CsvVenue>();
                csvWriter.WriteRecords(rows);
                csvWriter.Flush();
                return mem;
            }
        }
    }

    public class CsvVenue
    {
        public string VenueName { get; set; }
    }
}
