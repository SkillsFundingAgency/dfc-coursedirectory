using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Microsoft.Azure.WebJobs;

namespace Dfc.CourseDirectory.Functions
{
    public class SyncAllCosmosCollectionsToSql
    {
        private readonly SqlDataSync _sqlDataSync;

        public SyncAllCosmosCollectionsToSql(SqlDataSync sqlDataSync)
        {
            _sqlDataSync = sqlDataSync;
        }

        [FunctionName(nameof(SyncAllCosmosCollectionsToSql))]
        [NoAutomaticTrigger]
        public Task Run(string input) => _sqlDataSync.SyncAll();
    }
}
