using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
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
        internal const string FileName = "ONSPD_MAY_2023_UK.csv";
        internal const string EnglandCountryId = "E92000001";

        private const int ChunkSize = 200;

        private readonly BlobContainerClient _blobContainerClient;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ILogger<OnspdDataImporter> _logger;
        private HttpClient _httpClient;

        public OnspdDataImporter(
            BlobServiceClient blobServiceClient,
            ISqlQueryDispatcher sqlQueryDispatcher,
            ILogger<OnspdDataImporter> logger)
        {
            _blobContainerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _logger = logger;
            _httpClient = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(300)
            };
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

        public async Task AutomatedImportData()
        {
            string arcgisUrl = "https://www.arcgis.com/sharing/rest/search?f=json&filter=tags:\"PRD_ONSPD\"&sortField=created&sortOrder=desc&num=1";

            _logger.LogInformation("Automated process generate request url at: {arcgisUrl}", arcgisUrl);

            var response = await _httpClient.GetAsync(arcgisUrl);
            _logger.LogInformation("Response Code on GET from '{arcgisUrl}' is [{responseCode}]", arcgisUrl, response.StatusCode);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string responseFromServer = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonDocument.Parse(responseFromServer);
                if (jsonResponse.RootElement.TryGetProperty("results", out JsonElement dataElement))
                {
                    var importObjects = JsonSerializer.Deserialize<ImportObject[]>(dataElement);
                    if (importObjects != null)
                    {
                        var dataObject = importObjects[0];
                        var downloadLink = $"https://www.arcgis.com/sharing/content/items/{dataObject.id}/data";
                        _logger.LogInformation("Dataset found. Name: {name}. Title: {title}. Owner {Owner}", dataObject.name, dataObject.title, dataObject.owner);
                        //Download to temp folder and then insert data to Sql table
                        await DownloadZipFileToTempAsync(downloadLink);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to import / deserialize objects from {arcgisUrl}", arcgisUrl);
                    }
                }
            }
            else
            {
                _logger.LogWarning("Invalid Response code. Can not progress further. Import failed.");
            }
        }

        private async Task DownloadZipFileToTempAsync(string downloadLink)
        {
            _logger.LogInformation("Zip file Url :{downloadLink}", downloadLink);

            var extractDirectory = Path.Join(Path.GetTempPath(), "Onspd");
            Directory.CreateDirectory(extractDirectory);

            var downloadResponse = await _httpClient.GetAsync(downloadLink);

            _logger.LogInformation("Response Code on GET from '{downloadLink}' is [{responseCode}]", downloadLink, downloadResponse.StatusCode);

            if (downloadResponse.StatusCode == HttpStatusCode.OK)
            {
                var resultStream = await downloadResponse.Content.ReadAsStreamAsync();
                using var zip = new ZipArchive(resultStream);
                foreach (var entry in zip.Entries)
                {
                    if (entry.Name.EndsWith("_UK.csv") && entry.Name.StartsWith("ONSPD"))
                    {
                        _logger.LogInformation("Find csv file - {CsvName}.", entry.Name);
                        var destination = Path.Combine(extractDirectory, entry.Name);
                        _logger.LogInformation("Extract and saved in local drive.");
                        try
                        {
                            entry.ExtractToFile(destination, overwrite: true);
                            _logger.LogInformation("Extract csv file - {CsvName} complete.", entry.Name);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Extract csv file error message - {Message}.", ex.Message);
                        }

                        using StreamReader streamReader = new StreamReader(destination);
                        await ProcessCSVtoDBAsync(streamReader);

                        //Remove the CSV when process done
                        using FileStream fs = new FileStream(destination, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.RandomAccess | FileOptions.DeleteOnClose);
                        // temp file exists
                        _logger.LogInformation("Temp csv file - {CsvName} has been removed.", entry.Name);
                        break;
                    }
                }
            }
            else
            {
                _logger.LogWarning("Invalid Response code from '{downloadLink}'. Can not progress further. Import failed.", downloadLink);
            }

        }

        private async Task ProcessCSVtoDBAsync(StreamReader streamReader)
        {
            _logger.LogInformation($"Start import csv postcodes to DB.");
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
        private class ImportObject
        {
            public string id { get; set; }
            public string owner { get; set; }
            public string name { get; set; }
            public string title { get; set; }
        }
    }
}
