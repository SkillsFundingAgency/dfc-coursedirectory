using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Models;
using Query = Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.Queries.CreateStandard;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public partial class TestData
    {
        public async Task<Standard> CreateStandard(
            int standardCode,
            int version,
            string standardName,
            string notionalEndLevel = null,
            bool otherBodyApprovalRequired = false)
        {
            var id = Guid.NewGuid();

            await _cosmosDbQueryDispatcher.ExecuteQuery(
                new Query()
                {
                    Id = id,
                    StandardCode = standardCode,
                    Version = version,
                    StandardName = standardName,
                    NotionalEndLevel = notionalEndLevel,
                    OtherBodyApprovalRequired = otherBodyApprovalRequired ? "Y" : "N"
                });

            return new Standard()
            {
                StandardCode = standardCode,
                Version = version,
                StandardName = standardName,
                NotionalNVQLevelv2 = notionalEndLevel,
                OtherBodyApprovalRequired = otherBodyApprovalRequired
            };
        }
    }
}
