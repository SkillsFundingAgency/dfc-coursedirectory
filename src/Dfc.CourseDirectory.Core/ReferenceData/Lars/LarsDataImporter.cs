using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.ReferenceData.Lars
{
    public class LarsDataImporter
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public LarsDataImporter(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task ImportData()
        {
            await ImportAwardOrgCode();
            await ImportCategory();
            await ImportLearnAimRefType();
            await ImportLearningDelivery();
            await ImportLearningDeliveryCategory();
            await ImportSectorSubjectAreaTier1();
            await ImportSectorSubjectAreaTier2();

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

            Task ImportAwardOrgCode()
            {
                var records = ReadCsv<UpsertLarsAwardOrgCodesRecord>("AwardOrgCode.csv");

                return _sqlQueryDispatcher.ExecuteQuery(new UpsertLarsAwardOrgCodes()
                {
                    Records = records
                });
            }

            Task ImportCategory()
            {
                var records = ReadCsv<UpsertLarsCategoriesRecord>("Category.csv");

                return _sqlQueryDispatcher.ExecuteQuery(new UpsertLarsCategories()
                {
                    Records = records
                });
            }

            Task ImportLearnAimRefType()
            {
                var records = ReadCsv<UpsertLarsLearnAimRefTypesRecord>("LearnAimRefType.csv");

                return _sqlQueryDispatcher.ExecuteQuery(new UpsertLarsLearnAimRefTypes()
                {
                    Records = records
                });
            }

            Task ImportLearningDelivery()
            {
                var records = ReadCsv<UpsertLarsLearningDeliveriesRecord>("LearningDelivery.csv");

                return _sqlQueryDispatcher.ExecuteQuery(new UpsertLarsLearningDeliveries()
                {
                    Records = records
                });
            }

            Task ImportLearningDeliveryCategory()
            {
                var records = ReadCsv<UpsertLarsLearningDeliveryCategoriesRecord>("LearningDeliveryCategory.csv");

                return _sqlQueryDispatcher.ExecuteQuery(new UpsertLarsLearningDeliveryCategories()
                {
                    Records = records
                });
            }

            Task ImportSectorSubjectAreaTier1()
            {
                var records = ReadCsv<UpsertLarsSectorSubjectAreaTier1sRecord>("SectorSubjectAreaTier1.csv");

                return _sqlQueryDispatcher.ExecuteQuery(new UpsertLarsSectorSubjectAreaTier1s()
                {
                    Records = records
                });
            }

            Task ImportSectorSubjectAreaTier2()
            {
                var records = ReadCsv<UpsertLarsSectorSubjectAreaTier2sRecord>("SectorSubjectAreaTier2.csv");

                return _sqlQueryDispatcher.ExecuteQuery(new UpsertLarsSectorSubjectAreaTier2s()
                {
                    Records = records
                });
            }
        }
    }
}
