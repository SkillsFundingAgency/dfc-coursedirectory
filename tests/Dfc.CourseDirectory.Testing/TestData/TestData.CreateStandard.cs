using System;
using System.Data.SqlTypes;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Query = Dfc.CourseDirectory.Testing.DataStore.CosmosDb.Queries.CreateStandard;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        private static int _lastStandardCode = 1;

        public async Task<Standard> CreateStandard(
            int? standardCode = null,
            int? version = null,
            string standardName = "Test Standard",
            string notionalEndLevel = null,
            bool otherBodyApprovalRequired = false)
        {
            var id = Guid.NewGuid();
            notionalEndLevel ??= string.Empty;
            standardCode ??= Interlocked.Increment(ref _lastStandardCode);
            version ??= 1;

            await _cosmosDbQueryDispatcher.ExecuteQuery(
                new Query()
                {
                    Id = id,
                    StandardCode = standardCode.Value,
                    Version = version.Value,
                    StandardName = standardName,
                    NotionalEndLevel = notionalEndLevel,
                    OtherBodyApprovalRequired = otherBodyApprovalRequired ? "Y" : "N"
                });

            await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpsertLarsStandards()
            {
                Records = new[]
                {
                    new UpsertLarsStandardRecord()
                    {
                        StandardCode = standardCode.Value,
                        Version = version.Value,
                        StandardName = standardName,
                        NotionalEndLevel = notionalEndLevel,
                        OtherBodyApprovalRequired = otherBodyApprovalRequired,
                        EffectiveFrom = SqlDateTime.MinValue.Value,
                        UrlLink = string.Empty,
                        StandardSectorCode = string.Empty
                    }
                }
            }));

            return new Standard()
            {
                StandardCode = standardCode.Value,
                Version = version.Value,
                StandardName = standardName,
                NotionalNVQLevelv2 = notionalEndLevel,
                OtherBodyApprovalRequired = otherBodyApprovalRequired
            };
        }
    }
}
