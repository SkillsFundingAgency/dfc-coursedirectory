using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.Core.ReferenceData.Lars
{
    public class LarsDataImporter
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IClock _clock;

        public LarsDataImporter(
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            IServiceScopeFactory serviceScopeFactory,
            IClock clock)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _serviceScopeFactory = serviceScopeFactory;
            _clock = clock;
        }

        public async Task ImportData()
        {
            await ImportFrameworksToCosmos();
            await ImportProgTypesToCosmos();
            await ImportSectorSubjectAreaTier1sToCosmos();
            await ImportSectorSubjectAreaTier2sToCosmos();
            await ImportStandardsToCosmos();
            await ImportStandardSectorCodesToCosmos();

            await ImportAwardOrgCodeToSql();
            await ImportCategoryToSql();
            await ImportLearnAimRefTypeToSql();
            await ImportLearningDeliveryToSql();
            await ImportLearningDeliveryCategoryToSql();
            await ImportSectorSubjectAreaTier1ToSql();
            await ImportSectorSubjectAreaTier2ToSql();

            static IEnumerable<T> ReadCsv<T>(string fileName)
            {
                var assm = typeof(LarsDataImporter).Assembly;
                var resourcePath = $"{assm.GetName().Name}.ReferenceData.Lars.Data.{fileName}";

                using (var stream = assm.GetManifestResourceStream(resourcePath))
                using (var streamReader = new StreamReader(stream))
                using (var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
                {
                    return csvReader.GetRecords<T>().ToList();
                }
            }

            Task ImportFrameworksToCosmos()
            {
                var records = ReadCsv<FrameworkRow>("Framework.csv");

                return _cosmosDbQueryDispatcher.ExecuteQuery(new UpsertFrameworks()
                {
                    Now = _clock.UtcNow,
                    Records = records
                        .Where(r => r.EffectiveTo.HasValue)
                        .Select(r => new UpsertFrameworksRecord()
                        {
                            FrameworkCode = r.FworkCode,
                            ProgType = r.ProgType,
                            PathwayCode = r.PwayCode,
                            PathwayName = r.PathwayName,
                            NasTitle = r.NASTitle,
                            EffectiveFrom = r.EffectiveFrom,
                            EffectiveTo = r.EffectiveTo.Value,
                            SectorSubjectAreaTier1 = r.SectorSubjectAreaTier1,
                            SectorSubjectAreaTier2 = r.SectorSubjectAreaTier2
                        })
                        .Where(r => r.EffectiveTo > _clock.UtcNow.Date)
                });
            }

            Task ImportProgTypesToCosmos()
            {
                var records = ReadCsv<ProgTypeRow>("ProgType.csv");

                return _cosmosDbQueryDispatcher.ExecuteQuery(new UpsertProgTypes()
                {
                    Now = _clock.UtcNow,
                    Records = records
                        .Where(r => !r.ProgTypeDesc.StartsWith("T Level", StringComparison.InvariantCultureIgnoreCase))
                        .Select(r => new UpsertProgTypesRecord
                        {
                            ProgTypeId = r.ProgType,
                            ProgTypeDesc = r.ProgTypeDesc,
                            ProgTypeDesc2 = r.ProgTypeDesc2,
                            EffectiveFrom = r.EffectiveFrom,
                            EffectiveTo = r.EffectiveTo
                        })
                });
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

            Task ImportCategoryToSql()
            {
                var records = ReadCsv<UpsertLarsCategoriesRecord>("Category.csv");

                return WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpsertLarsCategories()
                {
                    Records = records
                }));
            }

            Task ImportLearnAimRefTypeToSql()
            {
                var records = ReadCsv<UpsertLarsLearnAimRefTypesRecord>("LearnAimRefType.csv");

                return WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpsertLarsLearnAimRefTypes()
                {
                    Records = records.Where(r => !r.LearnAimRefTypeDesc.StartsWith("T Level", StringComparison.InvariantCultureIgnoreCase))
                }));
            }

            Task ImportLearningDeliveryToSql()
            {
                var records = ReadCsv<UpsertLarsLearningDeliveriesRecord>("LearningDelivery.csv");

                return WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpsertLarsLearningDeliveries()
                {
                    Records = records.Where(r => !r.LearnAimRefTitle.StartsWith("T Level", StringComparison.InvariantCultureIgnoreCase))
                }));
            }

            Task ImportLearningDeliveryCategoryToSql()
            {
                var records = ReadCsv<UpsertLarsLearningDeliveryCategoriesRecord>("LearningDeliveryCategory.csv");

                return WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpsertLarsLearningDeliveryCategories()
                {
                    Records = records
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
                using var scope = _serviceScopeFactory.CreateScope();
                var dispatcher = scope.ServiceProvider.GetRequiredService<ISqlQueryDispatcher>();

                await action(dispatcher);

                await dispatcher.Transaction.CommitAsync();
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
    }
}
