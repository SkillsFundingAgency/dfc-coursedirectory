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
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Core.ReferenceData.Lars
{
    public class LarsDataImporter
    {
        private readonly LarsDataset _larsDataset;
        private readonly HttpClient _httpClient;
        private readonly ISqlQueryDispatcherFactory _sqlQueryDispatcherFactory;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly IClock _clock;
        private readonly ILogger<LarsDataImporter> _logger;

        public LarsDataImporter(
            HttpClient httpClient,
            ISqlQueryDispatcherFactory sqlQueryDispatcherFactory,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            IClock clock,
            ILogger<LarsDataImporter> logger,
            IOptions<LarsDataset> larsDatasetOption)
        {
            _httpClient = httpClient;
            _sqlQueryDispatcherFactory = sqlQueryDispatcherFactory;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _clock = clock;
            _logger = logger;
            _larsDataset = larsDatasetOption.Value;
        }

        public async Task ImportData()
        {
            var extractDirectory = Path.Join(Path.GetTempPath(), "lars");
            Directory.CreateDirectory(extractDirectory);

            await DownloadFiles();

            await ImportProgTypesToCosmos();
            await ImportSectorSubjectAreaTier1sToCosmos();
            await ImportSectorSubjectAreaTier2sToCosmos();
            await ImportStandardsToCosmos();
            await ImportStandardSectorCodesToCosmos();

            await ImportAwardOrgCodeToSql();
            var categoriesRefs = await ImportCategoryToSql();
            var learningDeliveryRefs = await ImportLearningDeliveryToSql();
            await ImportLearnAimRefTypeToSql();
            await ImportLearningDeliveryCategoryToSql(categoriesRefs, learningDeliveryRefs);
            await ImportSectorSubjectAreaTier1ToSql();
            await ImportSectorSubjectAreaTier2ToSql();
            await ImportStandardsToSql();
            await ImportStandardSectorCodesToSql();
            await ImportValidityToSql();

            IEnumerable<T> ReadCsv<T>(string fileName, Action<CsvContext> configureContext = null)
            {
                var assm = typeof(LarsDataImporter).Assembly;
                var filePath = Path.Join(extractDirectory, fileName);

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

            Task ImportProgTypesToCosmos()
            {
                const string csv = "ProgType.csv";
                var records = ReadCsv<ProgTypeRow>(csv).ToList();

                var excluded = records.Where(IsTLevel).Select(r => r.ProgType);
                _logger.LogInformation($"{csv} - Excluded {nameof(ProgTypeRow.ProgType)}s: {string.Join(",", excluded)} (T Level detected in {nameof(ProgTypeRow.ProgTypeDesc)})");

                return _cosmosDbQueryDispatcher.ExecuteQuery(new UpsertProgTypes()
                {
                    Now = _clock.UtcNow,
                    Records = records
                        .Where(r => !IsTLevel(r))
                        .Select(r => new UpsertProgTypesRecord
                        {
                            ProgTypeId = r.ProgType,
                            ProgTypeDesc = r.ProgTypeDesc,
                            ProgTypeDesc2 = r.ProgTypeDesc2,
                            EffectiveFrom = r.EffectiveFrom,
                            EffectiveTo = r.EffectiveTo
                        })
                });

                static bool IsTLevel(ProgTypeRow r) => r.ProgTypeDesc.StartsWith("T Level", StringComparison.InvariantCultureIgnoreCase);
            }

            Task ImportStandardsToCosmos()
            {
                var records = ReadCsv<UpsertStandardsRecord>("Standard.csv");

                return _cosmosDbQueryDispatcher.ExecuteQuery(new UpsertStandards()
                {
                    Records = records
                });
            }

            Task ImportStandardSectorCodesToCosmos()
            {
                var records = ReadCsv<StandardSectorCodeRow>("StandardSectorCode.csv");

                return _cosmosDbQueryDispatcher.ExecuteQuery(new UpsertStandardSectorCodes()
                {
                    Now = _clock.UtcNow,
                    Records = records.Select(r => new UpsertStandardSectorCodesRecord()
                    {
                        StandardSectorCodeId = r.StandardSectorCode.ToString(),
                        StandardSectorCodeDesc = r.StandardSectorCodeDesc,
                        StandardSectorCodeDesc2 = r.StandardSectorCodeDesc2,
                        EffectiveFrom = r.EffectiveFrom,
                        EffectiveTo = r.EffectiveTo
                    })
                });
            }

            Task ImportSectorSubjectAreaTier1sToCosmos()
            {
                var records = ReadCsv<SectorSubjectAreaTier1Row>("SectorSubjectAreaTier1.csv");

                return _cosmosDbQueryDispatcher.ExecuteQuery(new UpsertSectorSubjectAreaTier1s()
                {
                    Now = _clock.UtcNow,
                    Records = records.Select(r => new UpsertSectorSubjectAreaTier1sRecord()
                    {
                        SectorSubjectAreaTier1Id = r.SectorSubjectAreaTier1,
                        SectorSubjectAreaTier1Desc = r.SectorSubjectAreaTier1Desc,
                        SectorSubjectAreaTier1Desc2 = r.SectorSubjectAreaTier1Desc2,
                        EffectiveFrom = r.EffectiveFrom,
                        EffectiveTo = r.EffectiveTo
                    })
                });
            }

            Task ImportSectorSubjectAreaTier2sToCosmos()
            {
                var records = ReadCsv<SectorSubjectAreaTier2Row>("SectorSubjectAreaTier2.csv");

                return _cosmosDbQueryDispatcher.ExecuteQuery(new UpsertSectorSubjectAreaTier2s()
                {
                    Now = _clock.UtcNow,
                    Records = records.Select(r => new UpsertSectorSubjectAreaTier2sRecord()
                    {
                        SectorSubjectAreaTier2Id = r.SectorSubjectAreaTier2,
                        SectorSubjectAreaTier2Desc = r.SectorSubjectAreaTier2Desc,
                        SectorSubjectAreaTier2Desc2 = r.SectorSubjectAreaTier2Desc2,
                        EffectiveFrom = r.EffectiveFrom,
                        EffectiveTo = r.EffectiveTo
                    })
                });
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
                _logger.LogInformation($"{csv} - Excluded {nameof(UpsertLarsLearnAimRefTypesRecord.LearnAimRefType)}s: {string.Join(",", excluded)} (T Level detected in {nameof(UpsertLarsLearnAimRefTypesRecord.LearnAimRefTypeDesc)})");

                return WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpsertLarsLearnAimRefTypes()
                {
                    Records = records.Where(r => !IsTLevel(r))
                }));

                static bool IsTLevel(UpsertLarsLearnAimRefTypesRecord r) => r.LearnAimRefTypeDesc.StartsWith("T Level", StringComparison.InvariantCultureIgnoreCase);
            }

            async Task<HashSet<string>> ImportLearningDeliveryToSql()
            {
                const string csv = "LearningDelivery.csv";
                var records = ReadCsv<UpsertLarsLearningDeliveriesRecord>(csv);

                var excluded = records.Where(IsTLevel).Select(r => r.LearnAimRef);
                _logger.LogInformation($"{csv} - Excluded {nameof(UpsertLarsLearningDeliveriesRecord.LearnAimRef)}s: {string.Join(",", excluded)} (T Level detected in {nameof(UpsertLarsLearningDeliveriesRecord.LearnAimRefTitle)})");

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
                const string csv = "LearningDeliveryCategory.csv";
                var records = ReadCsv<UpsertLarsLearningDeliveryCategoriesRecord>(
                    csv,
                    configuration => configuration.RegisterClassMap<UpsertLarsLearningDeliveryCategoriesRecordClassMap>());

                // check referential integrity
                var validRecords = records.Where(r =>
                    categoriesRefs.Contains(r.CategoryRef) && learningDeliveryRefs.Contains(r.LearnAimRef));

                _logger.LogInformation($"{csv} - Excluded {records.Count() - validRecords.Count()} of {records.Count()} rows due to referential integrity violations");

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

            Task ImportStandardsToSql()
            {
                var records = ReadCsv<UpsertLarsStandardRecord>("Standard.csv", config => config.RegisterClassMap<UpsertLarsStandardRecord.ClassMap>());

                return WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpsertLarsStandards
                {
                    Records = records
                }));
            }

            Task ImportStandardSectorCodesToSql()
            {
                var records = ReadCsv<UpsertLarsStandardSectorCodeRecord>("StandardSectorCode.csv");

                return WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpsertLarsStandardSectorCodes
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
                _logger.LogInformation($"Start import validity.csv");
                var records = ReadCsv<UpsertLarsValidityRecord>(
                    "Validity.csv",
                    configuration => configuration.RegisterClassMap<UpsertLarsValidityRecordClassMap>());


                return WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpsertLarsValidity
                {
                    Records = records
                }));
            }
        }

        private class FrameworkRow
        {
            public int FworkCode { get; set; }
            public int ProgType { get; set; }
            public int PwayCode { get; set; }
            public string PathwayName { get; set; }
            public string NASTitle { get; set; }
            public DateTime EffectiveFrom { get; set; }
            public DateTime? EffectiveTo { get; set; }
            public decimal SectorSubjectAreaTier1 { get; set; }
            public decimal SectorSubjectAreaTier2 { get; set; }
        }

        private class ProgTypeRow
        {
            public int ProgType { get; set; }
            public string ProgTypeDesc { get; set; }
            public string ProgTypeDesc2 { get; set; }
            public DateTime EffectiveFrom { get; set; }
            public DateTime? EffectiveTo { get; set; }
        }

        private class SectorSubjectAreaTier1Row
        {
            public decimal SectorSubjectAreaTier1 { get; set; }
            public string SectorSubjectAreaTier1Desc { get; set; }
            public string SectorSubjectAreaTier1Desc2 { get; set; }
            public DateTime EffectiveFrom { get; set; }
            public DateTime? EffectiveTo { get; set; }
        }

        private class SectorSubjectAreaTier2Row
        {
            public decimal SectorSubjectAreaTier2 { get; set; }
            public string SectorSubjectAreaTier2Desc { get; set; }
            public string SectorSubjectAreaTier2Desc2 { get; set; }
            public DateTime EffectiveFrom { get; set; }
            public DateTime? EffectiveTo { get; set; }
        }

        private class StandardSectorCodeRow
        {
            public int StandardSectorCode { get; set; }
            public string StandardSectorCodeDesc { get; set; }
            public string StandardSectorCodeDesc2 { get; set; }
            public DateTime EffectiveFrom { get; set; }
            public DateTime? EffectiveTo { get; set; }
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

        private class UpsertLarsValidityRecordClassMap : ClassMap<UpsertLarsValidityRecord>
        {
            public UpsertLarsValidityRecordClassMap()
            {
                AutoMap(CultureInfo.InvariantCulture);
                Map(m => m.StartDate).TypeConverter<DateConverter>();
                Map(m => m.EndDate).TypeConverter<DateConverter>();
                Map(m => m.LastNewStartDate).TypeConverter<DateConverter>();
            }
        }
    }
}
