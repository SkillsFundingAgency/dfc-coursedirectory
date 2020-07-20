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
    public class SyncCosmosVenueToSql
    {
        private readonly SqlDataSync _sqlDataSync;

        public SyncCosmosVenueToSql(SqlDataSync sqlDataSync)
        {
            _sqlDataSync = sqlDataSync;
        }

        [FunctionName(nameof(SyncCosmosVenueToSql))]
        public async Task Run([CosmosDBTrigger(
            databaseName: "providerportal",
            collectionName: "venues",
            ConnectionStringSetting = "CosmosDbSettings:ConnectionString",
            LeaseCollectionName = "venues-lease",
            LeaseCollectionPrefix = "SyncCosmosVenueToSql",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> documents)
        {
            var venues = documents.Select(d => JsonConvert.DeserializeObject<Venue>(d.ToString()));

            await _sqlDataSync.SyncVenues(venues);
        }
    }
}
