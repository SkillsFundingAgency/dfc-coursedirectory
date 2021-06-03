using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.Models;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Courses
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
            var provider = await TestData.CreateProvider(providerType: ProviderType.FE);

            // Act
            var response = await HttpClient.GetAsync($"/data-upload/courses/template?providerId={provider.ProviderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("text/csv");
            response.Content.Headers.ContentDisposition.FileName.Should().Be("courses-template.csv");

            using var responseBody = await response.Content.ReadAsStreamAsync();
            using var responseBodyReader = new StreamReader(responseBody);
            using var csvReader = new CsvReader(responseBodyReader, CultureInfo.InvariantCulture);

            csvReader.Read();
            csvReader.ReadHeader();
            csvReader.Context.HeaderRecord.Should().BeEquivalentTo(new[]
            {
              "LARS_QAN",
              "WHO_THIS_COURSE_IS_FOR",
              "ENTRY_REQUIREMENTS",
              "WHAT_YOU_WILL_LEARN", 
              "HOW_YOU_WILL_LEARN",
              "WHAT_YOU_WILL_NEED_TO_BRING",
              "HOW_YOU_WILL_BE_ASSESSED",
              "WHERE_NEXT",
              "COURSE_NAME",
              "YOUR_REFERENCE",
              "DELIVERY_MODE",
              "START_DATE",
              "FLEXIBLE_START_DATE",
              "VENUE_NAME",
              "YOUR_VENUE_REFERENCE",
              "NATIONAL_DELIVERY",
              "SUB_REGION",  
              "COURSE_WEBPAGE", 
              "COST",
              "COST_DESCRIPTION",
              "DURATION",
              "DURATION_UNIT",
              "STUDY_MODE",
              "ATTENDANCE_PATTERN"
            });

            var rows = csvReader.GetRecords<CsvCourseRow>();
            rows.Should().BeEmpty();
        }
    }
}
