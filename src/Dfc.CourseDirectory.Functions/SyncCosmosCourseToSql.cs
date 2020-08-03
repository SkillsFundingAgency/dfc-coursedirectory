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
    public class SyncCosmosCourseToSql
    {
        private readonly SqlDataSync _sqlDataSync;

        public SyncCosmosCourseToSql(SqlDataSync sqlDataSync)
        {
            _sqlDataSync = sqlDataSync;
        }

        [FunctionName(nameof(SyncCosmosCourseToSql))]
        public async Task Run([CosmosDBTrigger(
            databaseName: "providerportal",
            collectionName: "courses",
            ConnectionStringSetting = "CosmosDbSettings:ConnectionString",
            LeaseCollectionName = "courses-lease",
            LeaseCollectionPrefix = "SyncCosmosCourseToSql",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> documents)
        {
            var courses = documents.Select(d => JsonConvert.DeserializeObject<Course>(d.ToString()));

            await _sqlDataSync.SyncCourses(courses);
        }
    }
}
