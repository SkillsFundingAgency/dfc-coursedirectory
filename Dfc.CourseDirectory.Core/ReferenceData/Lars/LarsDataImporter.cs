using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Dfc.CourseDirectory.Core.Configuration;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Core.ReferenceData.Lars
{
    public class LarsDataImporter
    {
        private readonly LarsDataset _larsDataset;
        private readonly HttpClient _httpClient;
        private readonly ISqlQueryDispatcherFactory _sqlQueryDispatcherFactory;
        private readonly ILogger<LarsDataImporter> _logger;
        private readonly BlobContainerClient _blobContainerClient;
        private string _larsApplicableFrom;
        private string _larsDateUploaded;
        private string _larsDownloadlink;

        public LarsDataImporter(
            HttpClient httpClient,
            ISqlQueryDispatcherFactory sqlQueryDispatcherFactory,
            ILogger<LarsDataImporter> logger,
            IOptions<LarsDataset> larsDatasetOption,
            BlobServiceClient blobServiceClient)
        {
            _httpClient = httpClient;
            _sqlQueryDispatcherFactory = sqlQueryDispatcherFactory;
            _logger = logger;
            _larsDataset = larsDatasetOption.Value;
            _blobContainerClient = blobServiceClient.GetBlobContainerClient(Constants.LarsContainerName);
        }
        private async Task GetDownloadFileDetails()
        {
            try
            {
                var link = _larsDataset.UrlSuffix;

                _httpClient.BaseAddress = new Uri(_larsDataset.Url);

                _logger.LogTrace("Lars baseurl {baseUrl}", _larsDataset.Url);

                var result = await _httpClient.GetStringAsync(link);

                var html = new HtmlDocument();
                html.LoadHtml(result);

                var table = html.DocumentNode.SelectSingleNode("//table[1]");
                var csvRow = table.SelectNodes("//td[1]")
                    .Where(x => x.InnerText == "CSV")
                    .First()
                    .ParentNode;

                _larsDateUploaded = csvRow
                    .SelectSingleNode("//td[3]")
                    .InnerHtml;
                _logger.LogTrace("Lars csv updated date {csvUpdated}", _larsDateUploaded);


                _larsApplicableFrom = csvRow
                    .SelectSingleNode("//td[2]")
                    .InnerHtml;
                _logger.LogTrace("Lars csv valid from date {validFrom}", _larsApplicableFrom);

                _larsDownloadlink = _larsDataset.Url + csvRow
                    .SelectSingleNode("//td[4]/a")
                    .GetAttributeValue("href", string.Empty);
                _logger.LogTrace("Lars csv download link {downloadLink}", _larsDownloadlink);

            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Error occurred retrieving Lars data file details.");
                throw;

            }
        }
        public async Task ImportData()
        {
            _logger.LogTrace("LarsDataImport started at: {time}", DateTimeOffset.Now);
       
            var _blobClient = _blobContainerClient.GetBlobClient(_larsDataset.DownloadInfo);
            var defaultDate = "01/01/1900";
            DateOnly lastDownloadDay;
            try
            {
                _logger.LogTrace("LarsDataImport started at: {time}", DateTimeOffset.Now);

                if (await _blobClient.ExistsAsync())
                {
                    var downloadInfoContent = await _blobClient.DownloadContentAsync();
                    var downloadInfoJson = downloadInfoContent.Value.Content.ToString();
                    var downloadInfo = JsonSerializer.Deserialize<Dictionary<string, string>>(downloadInfoJson);
                    defaultDate = downloadInfo["LastDownloadDate"];
                    _logger.LogTrace("Lars last downloaded on {downloadDate} ", defaultDate);
                }
                lastDownloadDay = DateOnly.ParseExact(defaultDate, "dd/MM/yyyy");
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Error occurred retrieving Blob storage data.");
                lastDownloadDay = DateOnly.ParseExact(defaultDate, "dd/MM/yyyy"); ;
            }

            try
            {
                await GetDownloadFileDetails();

                if (DateOnly.Parse(_larsDateUploaded).CompareTo(lastDownloadDay) > 0 && DateOnly.Parse(_larsApplicableFrom) <= DateOnly.FromDateTime(DateTime.Today))
                {

                    var request = new HttpRequestMessage(HttpMethod.Get, _larsDownloadlink);
                    var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Download link is not valid: {statusCode}", response.StatusCode);
                        throw new HttpRequestException($"Download link is not valid: {_larsDownloadlink}");
                    }
                    var data = await _httpClient.GetByteArrayAsync(_larsDownloadlink);
                    _logger.LogTrace("Lars new data found. Downloading from {downloadLink}", _larsDownloadlink);

                    var extractDirectory = Path.Join(Path.GetTempPath(), "lars");
                    Directory.CreateDirectory(extractDirectory);

                    await DownloadFiles(_larsDownloadlink);

                    await ImportValidityToSql();
                    await ImportAwardOrgCodeToSql();
                    var categoriesRefs = await ImportCategoryToSql();
                    var learningDeliveryRefs = await ImportLearningDeliveryToSql();
                    await ImportLearnAimRefTypeToSql();
                    await ImportLearningDeliveryCategoryToSql(categoriesRefs, learningDeliveryRefs);
                    await ImportSectorSubjectAreaTier1ToSql();
                    await ImportSectorSubjectAreaTier2ToSql();


                    IEnumerable<T> ReadCsv<T>(string fileName, Action<CsvContext> configureContext = null)

                    {
                        var assm = typeof(LarsDataImporter).Assembly;
                        var filePath = Path.Join(extractDirectory, fileName);
                        _logger.LogTrace("Read file - " + filePath);
                        using (var stream = File.OpenRead(filePath))
                        using (var streamReader = new StreamReader(stream))
                        using (var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
                        {
                            configureContext?.Invoke(csvReader.Context);

                            return csvReader.GetRecords<T>().ToList();
                        }
                    }

                    async Task DownloadFiles(string downloadFile)
                    {
                        _logger.LogTrace("Lars URL- " + downloadFile);
                        using var resultStream = await _httpClient.GetStreamAsync(downloadFile);
                        using var zip = new ZipArchive(resultStream);

                        foreach (var entry in zip.Entries)
                        {
                            if (entry.Name.EndsWith(".csv"))
                            {
                                var destination = Path.Combine(extractDirectory, entry.Name);
                                entry.ExtractToFile(destination, overwrite: true);
                            }
                        }
                    }


                    Task ImportAwardOrgCodeToSql()
                    {
                        var records = ReadCsv<UpsertLarsAwardOrgCodesRecord>(_larsDataset.AwardOrgCodeCsv);

                        return WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpsertLarsAwardOrgCodes()
                        {
                            Records = records
                        }));
                    }

                    async Task<HashSet<string>> ImportCategoryToSql()
                    {
                        var records = ReadCsv<UpsertLarsCategoriesRecord>(_larsDataset.CategoryCsv);

                        await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpsertLarsCategories()
                        {
                            Records = records
                        }));
                        return new HashSet<string>(records.Select(r => r.CategoryRef));
                    }

                    Task ImportLearnAimRefTypeToSql()
                    {
                        var records = ReadCsv<UpsertLarsLearnAimRefTypesRecord>(_larsDataset.LearnAimRefTypeCsv).ToList();

                        var excluded = records.Where(IsTLevel).Select(r => r.LearnAimRefType);
                        _logger.LogTrace("{csv} - Excluded {LearnAimRefType}s: {excluded} (T Level detected in {LearnAimRefTypeDesc})", _larsDataset.LearnAimRefTypeCsv, nameof(UpsertLarsLearnAimRefTypesRecord.LearnAimRefType), string.Join(",", excluded), nameof(UpsertLarsLearnAimRefTypesRecord.LearnAimRefTypeDesc));

                        return WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpsertLarsLearnAimRefTypes()
                        {
                            Records = records.Where(r => !IsTLevel(r))
                        }));

                        static bool IsTLevel(UpsertLarsLearnAimRefTypesRecord r) => r.LearnAimRefTypeDesc.StartsWith("T Level", StringComparison.InvariantCultureIgnoreCase);
                    }

                    async Task<HashSet<string>> ImportLearningDeliveryToSql()

                    {
                        var records = ReadCsv<UpsertLarsLearningDeliveriesRecord>(_larsDataset.LearningDeliveryCsv);

                        var excluded = records.Where(IsTLevel).Select(r => r.LearnAimRef);
                        _logger.LogInformation("{csv} - Excluded {LearnAimRef}s: {Excluded} (T Level detected in {LearnAimRefTitle})", _larsDataset.LearningDeliveryCsv, nameof(UpsertLarsLearningDeliveriesRecord.LearnAimRef), string.Join(", ", excluded), nameof(UpsertLarsLearningDeliveriesRecord.LearnAimRefTitle));

                        var includedRecords = records.Where(r => !IsTLevel(r)).ToList();
                        await WithSqlQueryDispatcher(dispatcher =>
                        {
                            return dispatcher.ExecuteQuery(new UpsertLarsLearningDeliveries()
                            {
                                Records = includedRecords
                            });
                        });


                        return new HashSet<string>(includedRecords.Select(r => r.LearnAimRef));

                        static bool IsTLevel(UpsertLarsLearningDeliveriesRecord r) => r.LearnAimRefTitle.StartsWith("T Level", StringComparison.InvariantCultureIgnoreCase);
                    }

                    Task ImportLearningDeliveryCategoryToSql(HashSet<string> categoriesRefs, HashSet<string> learningDeliveryRefs)
                    {
                        var records = ReadCsv<UpsertLarsLearningDeliveryCategoriesRecord>(
                            _larsDataset.LearningDeliveryCategoryCsv,
                            configuration => configuration.RegisterClassMap<UpsertLarsLearningDeliveryCategoriesRecordClassMap>());

                        // check referential integrity
                        var validRecords = records.Where(r =>
                            categoriesRefs.Contains(r.CategoryRef) && learningDeliveryRefs.Contains(r.LearnAimRef));

                        _logger.LogInformation("{csv} - Excluded {Count} of {ActualCount} rows due to referential integrity violations", _larsDataset.LearningDeliveryCategoryCsv, records.Count() - validRecords.Count(), records.Count());



                        return WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpsertLarsLearningDeliveryCategories()
                        {
                            Records = validRecords
                        }));
                    }

                    Task ImportSectorSubjectAreaTier1ToSql()
                    {
                        var records = ReadCsv<UpsertLarsSectorSubjectAreaTier1sRecord>(_larsDataset.SectorSubjectAreaTier1Csv);



                        return WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpsertLarsSectorSubjectAreaTier1s()
                        {
                            Records = records
                        }));
                    }

                    Task ImportSectorSubjectAreaTier2ToSql()
                    {
                        var records = ReadCsv<UpsertLarsSectorSubjectAreaTier2sRecord>(_larsDataset.SectorSubjectAreaTier2Csv);



                        return WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpsertLarsSectorSubjectAreaTier2s()
                        {
                            Records = records
                        }));
                    }



                    async Task WithSqlQueryDispatcher(Func<ISqlQueryDispatcher, Task> action)
                    {
                        using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();
                        await action(dispatcher);
                        await dispatcher.Commit();
                    }

                    Task ImportValidityToSql()
                    {
                        _logger.LogTrace("Start import validity.csv");
                        var records = ReadCsv<UpsertLarsValidityRecord>(_larsDataset.ValidityCsv);

                        _logger.LogTrace("Start import validity.csv records count - {Count} ", records.Count());

                        return WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpsertLarsValidity
                        {
                            Records = records
                        }));
                    }

                   // Get a reference to the container
                    var containerClient = _blobClient.GetParentBlobContainerClient();

                    // Check if the container exists, and create it if it doesn't
                    await containerClient.CreateIfNotExistsAsync();

                    // Update the blob storage with new download date
                    var updatedDownloadInfo = new Dictionary<string, string>
                    {
                        ["LastDownloadDate"] = DateTime.UtcNow.ToString("dd/MM/yyyy") // or use downloadDate if needed
                    };

                    // Serialize and upload the updated content
                    var updatedJson = JsonSerializer.Serialize(updatedDownloadInfo);
                    await _blobClient.UploadAsync(new BinaryData(updatedJson), overwrite: true);
                    _logger.LogInformation("LarsDataImport successfully completed");


                }
                else
                {
                    _logger.LogInformation("LarsDataImport did not run. Last uploaded on {downloadDate} ", lastDownloadDay);
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "LarsDataImport Error occurred during Lars data import" + ex.Message);
                throw;


            }
        }

        private class DateConverter : DefaultTypeConverter
        {
            public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return "";  // This is deliberately not null
                }

                // Sometimes we get '03 Aug 2015' format, other times '2015-08-03'
                // Normalize to '2015-08-03'
                // Validity date is '1/01/2001'

                var formats = new[] { "dd MMM yyyy", "yyyy-MM-dd", "dd-MM-yyyy" };
                var preferredFormat = "dd MMM yyyy";

                foreach (var format in formats)
                {
                    if (DateTime.TryParseExact(
                        text,
                        format,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var dt))
                    {
                        return dt.ToString(preferredFormat);
                    }
                }

                throw new TypeConverterException(
                    this,
                    memberMapData,
                    text,
                    row.Context,
                    $"Cannot parse date: '{text}'.");
            }

            public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
            {
                throw new NotImplementedException();
            }
        }

        private class UpsertLarsLearningDeliveryCategoriesRecordClassMap : ClassMap<UpsertLarsLearningDeliveryCategoriesRecord>
        {
            public UpsertLarsLearningDeliveryCategoriesRecordClassMap()
            {
                AutoMap(CultureInfo.InvariantCulture);
                Map(m => m.Created_On).TypeConverter<DateConverter>();
                Map(m => m.Modified_On).TypeConverter<DateConverter>();
                Map(m => m.EffectiveFrom).TypeConverter<DateConverter>();
                Map(m => m.EffectiveTo).TypeConverter<DateConverter>();
            }
        }
    }
}
