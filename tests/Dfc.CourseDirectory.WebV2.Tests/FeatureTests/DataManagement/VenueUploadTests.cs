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
        public VenueUploadTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
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
        public async Task Post_DataManagement_ValidVenuesFileRedirectsToPublish()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);
            HttpResponseMessage response;
            var requestContent = CreateMultiPartDataContent("text/csv");

            // Act
            response = await HttpClient.PostAsync("/data-upload/upload", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.Should().Be("/data-upload/venues/checkandpublish");
        }

        [Theory]
        [InlineData("application/vnd.ms-excel")]
        [InlineData("application/json")]
        [InlineData("text/plain")]
        public async Task Post_DataManagement_UnsupportedContentTypeFileRedirectsToValidationError(string contentType)
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);
            HttpResponseMessage response;
            var requestContent = CreateMultiPartDataContent(contentType);

            // Act
            response = await HttpClient.PostAsync("/data-upload/upload", requestContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.Should().Be("/data-upload/venues/validation");
        }

        private MultipartFormDataContent CreateMultiPartDataContent(string contentType)
        {
            var content = new MultipartFormDataContent("dfdfd");
            content.Headers.ContentType.MediaType = "multipart/form-data";
            using (var mem = new MemoryStream())
            using (var writer = new StreamWriter(mem))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteHeader<CsvVenue>();
                csvWriter.Flush();
                var byteArrayContent = new ByteArrayContent(mem.ToArray());
                byteArrayContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                content.Add(byteArrayContent, "this is the name of the content", "someFileName.csv");
            }
            return content;
        }
    }

    public class CsvVenue
    {
        public string VenueName { get; set; }
    }
}
