using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Core.ReferenceData.Onspd
{
    public class OnspdDataImporter
    {
        // internal for testing
        internal const string ContainerName = "onspd";
        internal const string FileName = "ONSPD_AUG_2022_UK.csv";
        internal const string EnglandCountryId = "E92000001";

        private const int ChunkSize = 200;

        private readonly BlobContainerClient _blobContainerClient;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ILogger<OnspdDataImporter> _logger;

        public OnspdDataImporter(
            BlobServiceClient blobServiceClient,
            ISqlQueryDispatcher sqlQueryDispatcher,
            ILogger<OnspdDataImporter> logger)
        {
            _blobContainerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _logger = logger;
        }

        public async Task ImportData()
        {
            var blobClient = _blobContainerClient.GetBlobClient(FileName);

            var blob = await blobClient.DownloadAsync();

            using var dataStream = blob.Value.Content;
            using var streamReader = new StreamReader(dataStream);
            using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);

            var rowCount = 0;
            var importedCount = 0;

            await foreach (var records in csvReader.GetRecordsAsync<Record>().Buffer(ChunkSize))
            {
                // Some data has invalid lat/lngs that will fail if we try to import..
                var withValidLatLngs = records
                    .Where(r => r.Latitude >= -90 && r.Latitude <= 90 && r.Longitude >= -90 && r.Longitude <= 90)
                    .ToArray();

                await _sqlQueryDispatcher.ExecuteQuery(new UpsertPostcodes()
                {
                    Records = withValidLatLngs
                        .Select(r => new UpsertPostcodesRecord()
                        {
                            Postcode = r.Postcode,
                            InEngland = r.Country == EnglandCountryId,
                            Position = (r.Latitude, r.Longitude)
                        })
                });

                rowCount += records.Count;
                importedCount += withValidLatLngs.Length;
            }

            _logger.LogInformation($"Processed {rowCount} rows, imported {importedCount} postcodes.");
        }

        private class Record
        {
            [Name("pcds")]
            public string Postcode { get; set; }

            [Name("lat")]
            public double Latitude { get; set; }

            [Name("long")]
            public double Longitude { get; set; }

            [Name("ctry")]
            public string Country { get; set; }
        }
    }
}
