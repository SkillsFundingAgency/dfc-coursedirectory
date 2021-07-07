using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using Dapper;
using Dfc.CourseDirectory.Core.ReferenceData.Campaigns;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.ReferenceDataTests
{
    public class CampaignDataImporterTests : DatabaseTestBase
    {
        public CampaignDataImporterTests(Testing.DatabaseTestBaseFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task MergesRowsFromCsv()
        {
            // Arrange
            var campaignCode = "test";

            var csvStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(csvStream, leaveOpen: true))
            using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteField("UKPRN");
                csvWriter.WriteField("LARS code");
                csvWriter.NextRecord();

                csvWriter.WriteField("12345");
                csvWriter.WriteField("ABC");
                csvWriter.NextRecord();

                csvWriter.WriteField("234567");
                csvWriter.WriteField("XYZ");
                csvWriter.NextRecord();
            }
            csvStream.Seek(0L, SeekOrigin.Begin);

            var importer = new CampaignDataImporter(SqlQueryDispatcherFactory);

            // Act
            await importer.ImportCampaignData(campaignCode, csvStream);

            // Assert
            await WithSqlQueryDispatcher(async dispatcher =>
            {
                var records = await dispatcher.Transaction.Connection.QueryAsync<Record>(
                    "SELECT * FROM Pttcd.CampaignProviderCourses ORDER BY ProviderUkprn",
                    transaction: dispatcher.Transaction);

                Assert.Collection(
                    records,
                    record =>
                    {
                        Assert.Equal(12345, record.ProviderUkprn);
                        Assert.Equal("ABC", record.LearnAimRef);
                    },
                    record =>
                    {
                        Assert.Equal(234567, record.ProviderUkprn);
                        Assert.Equal("XYZ", record.LearnAimRef);
                    });
            });
        }

        private class Record
        {
            public int ProviderUkprn { get; set; }
            public string LearnAimRef { get; set; }
        }
    }
}
