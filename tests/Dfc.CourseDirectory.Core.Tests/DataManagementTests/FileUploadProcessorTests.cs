using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using static Dfc.CourseDirectory.Core.DataManagement.FileUploadProcessor;

namespace Dfc.CourseDirectory.Core.Tests.DataManagementTests
{
    public partial class FileUploadProcessorTests : DatabaseTestBase
    {
        public FileUploadProcessorTests(DatabaseTestBaseFixture fixture) : base(fixture)
        {
            SqlQuerySpy = fixture.Services.GetRequiredService<SqlQuerySpy>();
        }

        public SqlQuerySpy SqlQuerySpy { get; }

        public static TheoryData<byte[], bool> LooksLikeCsvData { get; } = new TheoryData<byte[], bool>()
        {
            { Encoding.UTF8.GetBytes("first,second,third"), true },
            { Encoding.UTF8.GetBytes("first,second,third\n1,2,3"), true },
            { Convert.FromBase64String("77u/").Concat(Encoding.UTF8.GetBytes("first,second,third\n1,2,3")).ToArray(), true },  // Including BOM
            { Encoding.UTF8.GetBytes("abc\n"), false },

            // This data is a small PNG file
            { Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAABcAAAAbCAIAAAAYioOMAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAHYcAAB2HAY/l8WUAAAEkSURBVEhLY/hPDTBqCnYwgkz5tf/ge0sHIPqxai1UCDfAacp7Q8u3MipA9E5ZGyoEA7+vXPva0IJsB05TgJohpgARVOj//++LlgF1wsWBCGIHTlO+TZkBVwoV6Z0IF0FGQCkUU36fPf8pJgkeEMjqcBkBVA+URZjy99UruC+ADgGKwJV+a++GsyEIGGpfK2t/HTsB0YswBRhgcEUQ38K5yAhrrCFMgUcKBAGdhswFIjyxjjAFTc87LSMUrrL2n9t3oUoxAE5T0BAkpHABqCmY7kdGn5MzIcpwAagpyEGLiSBq8AAGzOQIQT937IKzoWpxAwa4UmQESUtwLkQpHgA1BS0VQQBppgBt/vfjB1QACZBmClYjgIA0UwgiqFrcgBqm/P8PAGN09WCiWJ70AAAAAElFTkSuQmCC"), false },
        };

        [Theory]
        [InlineData("", true)]
        [InlineData("77u/", true)]  // UTF-8 BOM
        [InlineData("Zmlyc3Qsc2Vjb25kLHRoaXJk", false)]  // "first,second,third"
        public async Task FileIsEmpty_ReturnsExpectedResult(string base64Content, bool expectedResult)
        {
            // Arrange
            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                Mock.Of<BlobServiceClient>(),
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var stream = new MemoryStream(Convert.FromBase64String(base64Content));
            stream.Seek(0L, SeekOrigin.Begin);

            // Act
            var result = await fileUploadProcessor.FileIsEmpty(stream);

            // Assert
            result.Should().Be(expectedResult);
        }

        [Theory]
        [MemberData(nameof(LooksLikeCsvData))]
        public async Task LooksLikeCsv_ReturnsExpectedResult(byte[] content, bool expectedResult)
        {
            // Arrange
            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                Mock.Of<BlobServiceClient>(),
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var stream = new MemoryStream(content);
            stream.Seek(0L, SeekOrigin.Begin);

            // Act
            var result = await fileUploadProcessor.LooksLikeCsv(stream);

            // Assert
            result.Should().Be(expectedResult);
        }

        [Fact]
        public async Task FileMatchesSchema_HeaderHasMissingColumn_ReturnsInvalidHeaderResult()
        {
            // Arrange
            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                Mock.Of<BlobServiceClient>(),
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var stream = DataManagementFileHelper.CreateCsvStream(csvWriter =>
            {
                // Miss out VENUE_NAME, POSTCODE
                csvWriter.WriteField("YOUR_VENUE_REFERENCE");
                csvWriter.WriteField("ADDRESS_LINE_1");
                csvWriter.WriteField("ADDRESS_LINE_2");
                csvWriter.WriteField("TOWN_OR_CITY");
                csvWriter.WriteField("COUNTY");
                csvWriter.WriteField("EMAIL");
                csvWriter.WriteField("PHONE");
                csvWriter.WriteField("WEBSITE");
                csvWriter.NextRecord();
            });

            // Act
            var (result, missingHeaders) = await fileUploadProcessor.FileMatchesSchema<CsvVenueRow>(stream);

            // Assert
            result.Should().Be(FileMatchesSchemaResult.InvalidHeader);
            missingHeaders.Should().BeEquivalentTo(new[]
            {
                "VENUE_NAME",
                "POSTCODE"
            });
        }

        [Theory]
        [InlineData(1)]  // Less than valid row
        //[InlineData(99]  // More than valid row - we don't have a way of checking this currently
        public async Task FileMatchesSchema_RowHasIncorrectColumnCount_ReturnsInvalidRows(int columnCount)
        {
            // Arrange
            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                Mock.Of<BlobServiceClient>(),
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var stream = DataManagementFileHelper.CreateVenueUploadCsvStream(csvWriter =>
            {
                for (int i = 0; i < columnCount; i++)
                {
                    csvWriter.WriteField("value");
                }

                csvWriter.NextRecord();
            });

            // Act
            var (result, missingHeaders) = await fileUploadProcessor.FileMatchesSchema<CsvVenueRow>(stream);

            // Assert
            result.Should().Be(FileMatchesSchemaResult.InvalidRows);
        }

        [Fact]
        public async Task CheckLearnAimRefs_ReturnsExpectedResult()
        {
            // Arrange
            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                Mock.Of<BlobServiceClient>(),
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            // Add missing lars
            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;
            List<CsvCourseRow> courseUploadRows = DataManagementFileHelper.CreateCourseUploadRows(learnAimRef, 1).ToList();
            courseUploadRows.AddRange(DataManagementFileHelper.CreateCourseUploadRows("", 1).ToList());
            courseUploadRows.AddRange(DataManagementFileHelper.CreateCourseUploadRows(learnAimRef, 1).ToList());
            courseUploadRows.AddRange(DataManagementFileHelper.CreateCourseUploadRows("    ", 1).ToList());

            // Add invalid and expired lars
            var expiredLearnAimRef = (await TestData.CreateLearningDelivery(effectiveTo: DateTime.Now.AddDays(-1))).LearnAimRef;
            courseUploadRows.AddRange(DataManagementFileHelper.CreateCourseUploadRows("ABCDEFG", 1).ToList());
            courseUploadRows.AddRange(DataManagementFileHelper.CreateCourseUploadRows(expiredLearnAimRef, 1).ToList());
            courseUploadRows.AddRange(DataManagementFileHelper.CreateCourseUploadRows("GFEDCBA", 1).ToList());

            var stream = DataManagementFileHelper.CreateCourseUploadCsvStream(courseUploadRows.ToArray());

            // Act
            var (missing, invalid, expired) = await fileUploadProcessor.CheckLearnAimRefs(stream);

            // Assert
            missing.Should().BeEquivalentTo(new[]
            {
                "3",
                "5"
            });
            invalid.Should().BeEquivalentTo(new[]
            {
                "6",
                "8"
            });
            expired.Should().BeEquivalentTo(new[]
            {
                string.Format("Row {0}, expired code {1}", 7, expiredLearnAimRef)
            });
        }
    }
}
