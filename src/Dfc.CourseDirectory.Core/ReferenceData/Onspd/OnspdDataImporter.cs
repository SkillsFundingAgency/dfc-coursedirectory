using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Core.Configuration;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Microsoft.AspNetCore.Http;
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

        public async Task AutomatedImportData()
        {
            string geoportal_url = "https://geoportal.statistics.gov.uk/datasets/ons-postcode-directory-(month)-(year)(extra)/about";

            string requesturl = GenerateRequestURL(DateTime.Now.Month, DateTime.Now.Year, geoportal_url, "");

            _logger.LogInformation($"Automated process generate request url at: {requesturl}");

            bool urlexist = await CheckURLExistsAndProcessAsync(requesturl);
            if (!urlexist)
            {
                _logger.LogInformation($"Not found url at: {requesturl}");
                requesturl = GenerateRequestURL(DateTime.Now.Month, DateTime.Now.Year, geoportal_url, "-1");
                _logger.LogInformation($"Automated process generate request url at: {requesturl}");
                urlexist = await CheckURLExistsAndProcessAsync(requesturl);
                if (urlexist)
                {
                    _logger.LogInformation($"Find url at: {requesturl}");
                }
            }
            else
            {
                _logger.LogInformation($"Find url at: {requesturl}");
            }
        }

        private async Task<bool> CheckURLExistsAndProcessAsync(string requesturl)
        {
            bool returnvalue = false;
            // Create a request for the URL. 		
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(requesturl);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                returnvalue = true;
                
                // Read the content.
                string responseFromServer = await response.Content.ReadAsStringAsync();
                string arcgisurl = "https://www.arcgis.com/sharing/rest/content/items/";
                if (responseFromServer.Contains(arcgisurl))
                {
                    int findindex = responseFromServer.IndexOf(arcgisurl);
                    int arcgisurllength = arcgisurl.Length;
                    string zipfileurl = arcgisurl + responseFromServer.Substring(findindex + arcgisurllength, 32) + "/data";
                    _logger.LogInformation($"Find arcgis download url at: {zipfileurl}");

                    //Download to temp folder
                    await DownloadZipFileToTempAsync(zipfileurl);
                }
            }

            return returnvalue;
        }

        public static string GenerateRequestURL(int month,
                                         int year,
                                         string geoportal_url,
                                         string extra)
        {
            if (month == 1 || month == 12 || month == 11)
            {
                month = 11;
                if (month == 1)
                    year = year - 1;
            }
            else if (month == 2 || month == 3 || month == 4)
            {
                month = 2;
            }
            else if (month == 5 || month == 6 || month == 7)
            {
                month = 5;
            }
            else if (month == 8 || month == 9 || month == 10)
            {
                month = 8;
            }
            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month).ToLower();
            string returnstring = geoportal_url.Replace("(month)", monthName);
            returnstring = returnstring.Replace("(year)", year.ToString());
            returnstring = returnstring.Replace("(extra)", extra);
            return returnstring;
        }

        private async Task DownloadZipFileToTempAsync(string zipfileurl)
        {
            _logger.LogInformation($"Start download zip file - {zipfileurl}.");
            var extractDirectory = Path.Join(Path.GetTempPath(), "Onspd");
            Directory.CreateDirectory(extractDirectory);

            HttpClient _httpClient=new HttpClient();
            using var resultStream = await _httpClient.GetStreamAsync(zipfileurl);
            using var zip = new ZipArchive(resultStream);

            foreach (var entry in zip.Entries)
            {
                if (entry.Name.EndsWith("UK.csv"))
                {
                    _logger.LogInformation($"Find csv file - {entry.Name}.");
                    var destination = Path.Combine(extractDirectory, entry.Name);
                    entry.ExtractToFile(destination, overwrite: true);
                    _logger.LogInformation($"Extract csv file - {entry.Name} complete.");

                    using StreamReader streamReader = new StreamReader(destination);
                    await ProcessCSVtoDBAsync(streamReader);
                }
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
    }
}
