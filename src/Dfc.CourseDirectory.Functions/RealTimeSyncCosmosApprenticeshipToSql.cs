using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.Functions
{
    public class RealTimeSyncCosmosApprenticeshipToSql
    {
        private readonly SqlDataSync _sqlDataSync;

        public RealTimeSyncCosmosApprenticeshipToSql(SqlDataSync sqlDataSync)
        {
            _sqlDataSync = sqlDataSync;
        }

        [FunctionName(nameof(RealTimeSyncCosmosApprenticeshipToSql))]
        public async Task Run([CosmosDBTrigger(
            databaseName: "providerportal",
            collectionName: "apprenticeship",
            ConnectionStringSetting = "CosmosDbSettings:ConnectionString",
            LeaseCollectionName = "apprenticeship-lease",
            LeaseCollectionPrefix = "SyncCosmosApprenticeshipToSql",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> documents)
        {
            var apprenticeships = documents.Select(d => JsonConvert.DeserializeObject<Apprenticeship>(d.ToString()));

            await _sqlDataSync.SyncApprenticeships(apprenticeships);
        }
    }
}
