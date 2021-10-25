using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Venues
{
    public class TemplateTests : MvcTestBase
    {
        public TemplateTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_DownloadsValidFile()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync($"/data-upload/venues/template");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.FileName.Should().Be("venues-template.csv");

            using var responseBody = await response.Content.ReadAsStreamAsync();
            using var responseBodyReader = new StreamReader(responseBody);
            using var csvReader = new CsvReader(responseBodyReader, CultureInfo.InvariantCulture);

            csvReader.Read();
            csvReader.ReadHeader();
            csvReader.Context.Reader.HeaderRecord.Should().BeEquivalentTo(new[]
            {
                "YOUR_VENUE_REFERENCE",
                "VENUE_NAME",
                "ADDRESS_LINE_1",
                "ADDRESS_LINE_2",
                "TOWN_OR_CITY",
                "COUNTY",
                "POSTCODE",
                "EMAIL",
                "PHONE",
                "WEBSITE"
            });

            var rows = csvReader.GetRecords<CsvVenueRow>();
            rows.Should().BeEmpty();
        }
    }
}
