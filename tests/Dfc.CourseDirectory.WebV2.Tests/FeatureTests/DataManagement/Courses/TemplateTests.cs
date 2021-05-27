using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
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

            // Act
            var response = await HttpClient.GetAsync($"/data-upload/courses/template");

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
                "WHO_IS_THIS_COURSE_FOR",
                "ENTRY_REQUIREMENTS",
                "WHAT_YOU_WILL_LEARN",
                "HOW_YOU_WILL_LEARN",
                "WHAT_YOU_WILL_NEED_TO_BRING",
                "HOW_YOU_WILL_BE_ASSESSED",
                "WHERE_NEXT",
                "ADVANCED_LEARNER_OPTION",
                "ADULT_EDUCATION_BUDGET",
                "COURSE_NAME",
                "ID",
                "DELIVERY_MODE",
                "START_DATE",
                "FLEXIBLE_START_DATE",
                "VENUE",
                "NATIONAL_DELIVERY",
                "REGION",
                "SUB_REGION",
                "URL",
                "COST",
                "COST_DESCRIPTION",
                "DURATION",
                "DURATION_UNIT",
                "STUDY_MODE",
                "ATTENDANCE_PATTERN"
            });

            var rows = csvReader.GetRecords<CourseRow>();
            rows.Should().BeEmpty();
        }
    }
}
