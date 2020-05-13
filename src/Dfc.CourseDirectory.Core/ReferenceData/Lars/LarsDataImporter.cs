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

namespace Dfc.CourseDirectory.Core.ReferenceData.Lars
{
    public class LarsDataImporter
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly IClock _clock;

        public LarsDataImporter(
            ISqlQueryDispatcher sqlQueryDispatcher,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            IClock clock)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
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
                var path = Path.Combine("ReferenceData\\Lars\\Data\\", fileName);

                if (!File.Exists(path))
                {
                    throw new ArgumentException($"File not found at path '{path}'.", nameof(fileName));
                }

                using (var reader = File.OpenText(path))
                using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
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
                    Records = records.Select(r => new UpsertFrameworksRecord()
                    {
                        FrameworkCode = r.FworkCode,
                        ProgType = r.ProgType,
                        PathwayCode = r.PwayCode,
                        PathwayName = r.PathwayName,
                        NasTitle = r.NASTitle,
                        EffectiveFrom = r.EffectiveFrom,
                        EffectiveTo = r.EffectiveTo
                    })
                });
            }

            Task ImportProgTypesToCosmos()
            {
                var records = ReadCsv<ProgTypeRow>("ProgType.csv");

                return _cosmosDbQueryDispatcher.ExecuteQuery(new UpsertProgTypes()
                {
                    Now = _clock.UtcNow,
                    Records = records.Select(r => new UpsertProgTypesRecord()
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

                return _sqlQueryDispatcher.ExecuteQuery(new UpsertLarsAwardOrgCodes()
                {
                    Records = records
                });
            }

            Task ImportCategoryToSql()
            {
                var records = ReadCsv<UpsertLarsCategoriesRecord>("Category.csv");

                return _sqlQueryDispatcher.ExecuteQuery(new UpsertLarsCategories()
                {
                    Records = records
                });
            }

            Task ImportLearnAimRefTypeToSql()
            {
                var records = ReadCsv<UpsertLarsLearnAimRefTypesRecord>("LearnAimRefType.csv");

                return _sqlQueryDispatcher.ExecuteQuery(new UpsertLarsLearnAimRefTypes()
                {
                    Records = records
                });
            }

            Task ImportLearningDeliveryToSql()
            {
                var records = ReadCsv<UpsertLarsLearningDeliveriesRecord>("LearningDelivery.csv");

                return _sqlQueryDispatcher.ExecuteQuery(new UpsertLarsLearningDeliveries()
                {
                    Records = records
                });
            }

            Task ImportLearningDeliveryCategoryToSql()
            {
                var records = ReadCsv<UpsertLarsLearningDeliveryCategoriesRecord>("LearningDeliveryCategory.csv");

                return _sqlQueryDispatcher.ExecuteQuery(new UpsertLarsLearningDeliveryCategories()
                {
                    Records = records
                });
            }

            Task ImportSectorSubjectAreaTier1ToSql()
            {
                var records = ReadCsv<UpsertLarsSectorSubjectAreaTier1sRecord>("SectorSubjectAreaTier1.csv");

                return _sqlQueryDispatcher.ExecuteQuery(new UpsertLarsSectorSubjectAreaTier1s()
                {
                    Records = records
                });
            }

            Task ImportSectorSubjectAreaTier2ToSql()
            {
                var records = ReadCsv<UpsertLarsSectorSubjectAreaTier2sRecord>("SectorSubjectAreaTier2.csv");

                return _sqlQueryDispatcher.ExecuteQuery(new UpsertLarsSectorSubjectAreaTier2s()
                {
                    Records = records
                });
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
            public DateTime EffectiveTo { get; set; }
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
