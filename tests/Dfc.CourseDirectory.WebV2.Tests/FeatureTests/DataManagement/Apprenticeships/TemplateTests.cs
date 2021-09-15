using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.Models;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Apprenticeships
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
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.GetAsync($"/data-upload/apprenticeships/template?providerId={provider.ProviderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.FileName.Should().Be("apprenticeships-template.csv");

            using var responseBody = await response.Content.ReadAsStreamAsync();
            using var responseBodyReader = new StreamReader(responseBody);
            using var csvReader = new CsvReader(responseBodyReader, CultureInfo.InvariantCulture);

            csvReader.Read();
            csvReader.ReadHeader();
            csvReader.Context.Reader.HeaderRecord.Should().BeEquivalentTo(new[]
            {
                "STANDARD_CODE",
                "STANDARD_VERSION",
                "APPRENTICESHIP_INFORMATION",
                "APPRENTICESHIP_WEBPAGE",
                "CONTACT_EMAIL",
                "CONTACT_PHONE",
                "CONTACT_URL",
                "DELIVERY_METHOD",
                "VENUE",
                "YOUR_VENUE_REFERENCE",
                "RADIUS",
                "DELIVERY_MODE",
                "NATIONAL_DELIVERY",
                "SUB_REGION"
            });

            var rows = csvReader.GetRecords<CsvApprenticeshipRow>();
            rows.Should().BeEmpty();
        }
    }
}
