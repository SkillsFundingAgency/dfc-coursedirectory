using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.Jobs
{
    public class RealTimeSyncCosmosUkrlpToSql
    {
        private readonly SqlDataSync _sqlDataSync;

        public RealTimeSyncCosmosUkrlpToSql(SqlDataSync sqlDataSync)
        {
            _sqlDataSync = sqlDataSync;
        }

        [FunctionName(nameof(RealTimeSyncCosmosUkrlpToSql))]
        public async Task Run([CosmosDBTrigger(
            databaseName: "providerportal",
            collectionName: "ukrlp",
            ConnectionStringSetting = "CosmosDbSettings:ConnectionString",
            LeaseCollectionName = "ukrlp-lease",
            LeaseCollectionPrefix = "SyncCosmosUkrlpToSql",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> documents)
        {
            var providers = documents.Select(d => JsonConvert.DeserializeObject<Provider>(d.ToString()));

            await _sqlDataSync.SyncProviders(providers);
        }
    }
}
