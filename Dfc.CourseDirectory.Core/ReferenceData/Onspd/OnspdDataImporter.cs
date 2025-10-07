using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
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
        internal const string EnglandCountryId = "E92000001";
        private const string LastDownloadDate = "LastDownloadDate";
        private const string OnsDownloadBlobFile = "OnsLastDownloadInfo.json";

        private const int ChunkSize = 10000;
        private const long DefaultEpochMs = -2208988800000; // "01/01/1900"

        private readonly BlobContainerClient _blobContainerClient; 
        private readonly ISqlQueryDispatcherFactory _sqlQueryDispatcherFactory;
        private readonly ILogger<OnspdDataImporter> _logger;
        private HttpClient _httpClient;
        private readonly BlobClient _lastDownloadDateBlobClient;


        public OnspdDataImporter(
            BlobServiceClient blobServiceClient,
            ISqlQueryDispatcherFactory sqlQueryDispatcherFactory,
            ILogger<OnspdDataImporter> logger)
        {
            _blobContainerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
            _sqlQueryDispatcherFactory = sqlQueryDispatcherFactory;
            _logger = logger;
            _httpClient = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(300)
            };
            _lastDownloadDateBlobClient = _blobContainerClient.GetBlobClient(OnsDownloadBlobFile);
        }

        public async Task ManualDataImport(string filename)
        {
            try
            {
                var blobClient = _blobContainerClient.GetBlobClient(filename);
                var blob = await blobClient.DownloadStreamingAsync();

                await using var dataStream = blob.Value.Content;
                using var streamReader = new StreamReader(dataStream);
                await ProcessCsvToDbAsync(streamReader);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error has occurred during {FunctionName}: {ErrorMessage}", nameof(OnspdDataImporter), ex.Message);
                throw;
            }
        }

        public async Task AutomatedDataImport()
        {
            try
            {
                var queryLink = "https://hub.arcgis.com/api/search/v1/collections/all/items?filter=((type%20IN%20(%27CSV%20Collection%27)))%20AND%20((categories%20IN%20(%27/categories/postcode%20products/ons%20postcode%20directory%27)))&limit=12&sortBy=-properties.created";
                _logger.LogTrace("Automated process generate request url at: {queryLink}", queryLink);
                var downloadLink = "https://www.arcgis.com/sharing/rest/content/items/{0}/data";

                var downloadDate = await GetDownloadDate();

                var onsMetadata = await _httpClient.GetFromJsonAsync<Feature>(queryLink);
                var latestModifiedOns = onsMetadata.Features.MaxBy(x => x.Properties.Created);

                if (latestModifiedOns.Properties.Modified.CompareTo(downloadDate) > 0)
                {
                    var datasetLink = string.Format(downloadLink, latestModifiedOns.Id);
                    _logger.LogTrace("New Version of ONS data found, beginning import with dataset: {datasetLink}", datasetLink);

                    await DownloadZipFileToTempAsync(datasetLink);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred importing ONS Data.");
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
                        await ProcessCsvToDbAsync(streamReader);

                        //Remove the CSV when process done
                        using FileStream fs = new FileStream(destination, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.RandomAccess | FileOptions.DeleteOnClose);
                        // temp file exists

                        //Update the blob storage with new download date
                        var updatedDownloadInfo = new Dictionary<string, string>
                        {
                            [LastDownloadDate] = DateTime.UtcNow.ToString("dd/MM/yyyy")
                        };

                        var updatedJson = JsonSerializer.Serialize(updatedDownloadInfo, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        await _lastDownloadDateBlobClient.UploadAsync(new BinaryData(updatedJson), overwrite: true);

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

        private async Task ProcessCsvToDbAsync(StreamReader streamReader)
        {
            using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            using var sqlDispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();

            var rowCount = 0;
            var importedCount = 0;

            await foreach (var records in csvReader.GetRecordsAsync<Record>().Buffer(ChunkSize))
            {
                // Some data has invalid lat/lngs that will fail if we try to import.
                var validRecords = records
                    .Where(IsValidRecord)
                    .ToArray();

                await sqlDispatcher.ExecuteQuery(new UpsertPostcodes()
                {
                    Records = validRecords
                        .Select(r => new UpsertPostcodesRecord()
                        {
                            Postcode = r.Postcode,
                            InEngland = r.Country == EnglandCountryId,
                            Position = (r.Latitude, r.Longitude)
                        })
                });

                rowCount += records.Count;
                importedCount += validRecords.Length;

                if (rowCount % 100000 == 0)
                {
                    _logger.LogInformation(
                        "Currently processed {RowCount} rows & imported {ImportedCount} postcodes",
                        rowCount, importedCount);
                }
            }
            await sqlDispatcher.Commit();

            _logger.LogInformation("Successfully processed {RowCount} rows & imported {ImportedCount} postcodes",
                rowCount, importedCount);
        }

        private async Task<long> GetDownloadDate()
        {
            try
            {
                if (!_lastDownloadDateBlobClient.Exists()) { return DefaultEpochMs; }

                var blobData = await _lastDownloadDateBlobClient.DownloadContentAsync();
                var LastOnsDownloadDate = JsonSerializer.Deserialize<Dictionary<string, string>>(blobData.Value.Content.ToString(), 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true});

                return DateTime.TryParse(LastOnsDownloadDate[LastDownloadDate], out var downloadDate)
                    ? ((DateTimeOffset)downloadDate).ToUnixTimeMilliseconds()
                    : DefaultEpochMs;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Error occurred retrieving Ons Blob storage data.");
                return DefaultEpochMs;
            }
        }

        private static bool IsValidRecord(Record record)
        {
            return record.Latitude >= -90 && record.Latitude <= 90 && record.Longitude >= -90 && record.Longitude <= 90;
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

        private class Ons
        {
            public string Id { get; set; }
            public Properties Properties { get; set; }
        }

        private class Feature
        {
            public List<Ons> Features { get; set; }
        }

        private class Properties
        {
            public long Created { get; set; }
            public long Modified { get; set; }
        }
    }
}
