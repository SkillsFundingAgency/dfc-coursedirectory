using System;
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
            var provider = await TestData.CreateProvider(providerName: "Test Provider");

            var venue1 = await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), venueName: Faker.Company.Name());
            var venue2 = await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), venueName: Faker.Company.Name());
            var venue3 = await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), venueName: Faker.Company.Name());

            Clock.UtcNow = new DateTime(2021, 4, 9, 13, 0, 0);

            // Act
            var response = await HttpClient.GetAsync($"/data-upload/venues/download?providerId={provider.ProviderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.FileName.Should().Be("\"Test Provider_venues_202104091300.csv\"");

            using var responseBody = await response.Content.ReadAsStreamAsync();
            using var responseBodyReader = new StreamReader(responseBody);
            using var csvReader = new CsvReader(responseBodyReader, CultureInfo.InvariantCulture);

            csvReader.Read();
            csvReader.ReadHeader();
            csvReader.Context.HeaderRecord.Should().BeEquivalentTo(new[]
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

            var rows = csvReader.GetRecords<VenueRow>();
            rows.Should().BeEquivalentTo(
                new[]
                {
                    venue1,
                    venue2,
                    venue3
                }
                .OrderBy(v => v.ProviderVenueRef)
                .ThenBy(v => v.VenueName)
                .Select(v => new VenueRow()
                {
                    AddressLine1 = v.AddressLine1,
                    AddressLine2 = v.AddressLine2,
                    County = v.County,
                    Email = v.Email,
                    Postcode = v.Postcode,
                    ProviderVenueRef = v.ProviderVenueRef ?? string.Empty,
                    Telephone = v.Telephone,
                    Town = v.Town,
                    VenueName = v.VenueName,
                    Website = v.Website
                }));
        }
    }
}
