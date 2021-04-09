using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Venues
{
    public class DownloadTests : MvcTestBase
    {
        public DownloadTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_DownloadsValidFile()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var venue1 = await TestData.CreateVenue(provider.ProviderId, venueName: Faker.Company.Name());
            var venue2 = await TestData.CreateVenue(provider.ProviderId, venueName: Faker.Company.Name());
            var venue3 = await TestData.CreateVenue(provider.ProviderId, venueName: Faker.Company.Name());

            // Act
            var response = await HttpClient.GetAsync($"/data-upload/venues/download?providerId={provider.ProviderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("text/csv");

            using var responseBody = await response.Content.ReadAsStreamAsync();
            using var responseBodyReader = new StreamReader(responseBody);
            using var csvReader = new CsvReader(responseBodyReader, CultureInfo.InvariantCulture);

            var rows = csvReader.GetRecords<VenueRow>();
            rows.Should().BeEquivalentTo(
                new[]
                {
                    venue1,
                    venue2,
                    venue3
                }
                .OrderBy(v => v.ProvVenueID)
                .ThenBy(v => v.VenueName)
                .Select(v => new VenueRow()
                {
                    AddressLine1 = v.AddressLine1,
                    AddressLine2 = v.AddressLine2,
                    County = v.County,
                    Email = v.Email,
                    Postcode = v.Postcode,
                    ProviderVenueRef = v.ProvVenueID ?? string.Empty,
                    Telephone = v.PHONE,
                    Town = v.Town,
                    VenueName = v.VenueName,
                    Website = v.Website
                }));
        }
    }
}
