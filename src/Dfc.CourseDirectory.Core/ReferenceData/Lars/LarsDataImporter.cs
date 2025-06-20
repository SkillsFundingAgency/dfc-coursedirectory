using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Dfc.CourseDirectory.Core.Configuration;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
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

        public LarsDataImporter(
            HttpClient httpClient,
            ISqlQueryDispatcherFactory sqlQueryDispatcherFactory,
            ILogger<LarsDataImporter> logger,
            IOptions<LarsDataset> larsDatasetOption)
        {
            _httpClient = httpClient;
            _sqlQueryDispatcherFactory = sqlQueryDispatcherFactory;
            _logger = logger;
            _larsDataset = larsDatasetOption.Value;
        }

        public async Task ImportData()
        {
            var extractDirectory = Path.Join(Path.GetTempPath(), "lars");
            Directory.CreateDirectory(extractDirectory);

            await DownloadFiles();

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
                _logger.LogInformation("Read file - "+ filePath);
                using (var stream = File.OpenRead(filePath))
                using (var streamReader = new StreamReader(stream))
                using (var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
                {
                    configureContext?.Invoke(csvReader.Context);

                    return csvReader.GetRecords<T>().ToList();
                }
            }

            async Task DownloadFiles()
            {
                _logger.LogInformation("Lars URL- " + _larsDataset.Url);
                using var resultStream = await _httpClient.GetStreamAsync(_larsDataset.Url);
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
                var records = ReadCsv<UpsertLarsAwardOrgCodesRecord>("AwardOrgCode.csv");

                return WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpsertLarsAwardOrgCodes()
                {
                    Records = records
                }));
            }

            async Task<HashSet<string>> ImportCategoryToSql()
            {
                var records = ReadCsv<UpsertLarsCategoriesRecord>("Category.csv");

                await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpsertLarsCategories()
                {
                    Records = records
                }));
                return new HashSet<string>(records.Select(r => r.CategoryRef));
            }

            Task ImportLearnAimRefTypeToSql()
            {
                const string csv = "LearnAimRefType.csv";
                var records = ReadCsv<UpsertLarsLearnAimRefTypesRecord>(csv).ToList();

                var excluded = records.Where(IsTLevel).Select(r => r.LearnAimRefType);
                _logger.LogInformation("{csv} - Excluded {LearnAimRefType}s: {excluded} (T Level detected in {LearnAimRefTypeDesc})", csv, nameof(UpsertLarsLearnAimRefTypesRecord.LearnAimRefType), string.Join(",", excluded), nameof(UpsertLarsLearnAimRefTypesRecord.LearnAimRefTypeDesc));

                return WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpsertLarsLearnAimRefTypes()
                {
                    Records = records.Where(r => !IsTLevel(r))
                }));

                static bool IsTLevel(UpsertLarsLearnAimRefTypesRecord r) => r.LearnAimRefTypeDesc.StartsWith("T Level", StringComparison.InvariantCultureIgnoreCase);
            }

            async Task<HashSet<string>> ImportLearningDeliveryToSql()
            {
                var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["X"] = "Not Applicable / Not Known"
                };

                const string csv = "LearningDelivery.csv";
                var records = ReadCsv<UpsertLarsLearningDeliveriesRecord>(csv);

                var excluded = records.Where(IsTLevel).Select(r => r.LearnAimRef);
                _logger.LogInformation("{csv} - Excluded {LearnAimRef}s: {Excluded} (T Level detected in {LearnAimRefTitle})", csv, nameof(UpsertLarsLearningDeliveriesRecord.LearnAimRef), string.Join(", ", excluded), nameof(UpsertLarsLearningDeliveriesRecord.LearnAimRefTitle));

                List<UpsertLarsLearningDeliveriesRecord> includedRecords = records.Where(r => !IsTLevel(r)).ToList();
                
                includedRecords.ForEach(r =>
                {
                    r.NotionalNVQLevel = mappings.GetValueOrDefault(r.NotionalNVQLevel, r.NotionalNVQLevel);
                    r.NotionalNVQLevelv2 = mappings.GetValueOrDefault(r.NotionalNVQLevelv2, r.NotionalNVQLevelv2);
                });

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
                const string csv = "LearningDeliveryCategory.csv";
                var records = ReadCsv<UpsertLarsLearningDeliveryCategoriesRecord>(
                    csv,
                    configuration => configuration.RegisterClassMap<UpsertLarsLearningDeliveryCategoriesRecordClassMap>());

                // check referential integrity
                var validRecords = records.Where(r =>
                    categoriesRefs.Contains(r.CategoryRef) && learningDeliveryRefs.Contains(r.LearnAimRef));

                _logger.LogInformation("{csv} - Excluded {Count} of {ActualCount} rows due to referential integrity violations",csv, records.Count() - validRecords.Count(), records.Count());

                return WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpsertLarsLearningDeliveryCategories()
                {
                    Records = validRecords
                }));
            }

            Task ImportSectorSubjectAreaTier1ToSql()
            {
                var records = ReadCsv<UpsertLarsSectorSubjectAreaTier1sRecord>("SectorSubjectAreaTier1.csv");

                return WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpsertLarsSectorSubjectAreaTier1s()
                {
                    Records = records
                }));
            }

            Task ImportSectorSubjectAreaTier2ToSql()
            {
                var records = ReadCsv<UpsertLarsSectorSubjectAreaTier2sRecord>("SectorSubjectAreaTier2.csv");

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
                _logger.LogInformation("Start import validity.csv");
                var records = ReadCsv<UpsertLarsValidityRecord>("Validity.csv");

                _logger.LogInformation("Start import validity.csv records count - {Count} ",records.Count());

                return WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpsertLarsValidity
                {
                    Records = records
                }));
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

                var formats = new[] { "dd MMM yyyy", "yyyy-MM-dd","dd-MM-yyyy" };
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
