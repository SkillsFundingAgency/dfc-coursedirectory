using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Microsoft.Azure.WebJobs;

namespace Dfc.CourseDirectory.Functions
{
    public class SyncCosmosCollectionsToSql
    {
        private readonly SqlDataSync _sqlDataSync;

        public SyncCosmosCollectionsToSql(SqlDataSync sqlDataSync)
        {
            _sqlDataSync = sqlDataSync;
        }

        [FunctionName(nameof(SyncAllCosmosCollectionsToSql))]
        [NoAutomaticTrigger]
        public Task SyncAllCosmosCollectionsToSql(string input) => _sqlDataSync.SyncAll();

        [FunctionName(nameof(SyncApprenticeshipsCosmosCollectionToSql))]
        [NoAutomaticTrigger]
        public Task SyncApprenticeshipsCosmosCollectionToSql(string input) => _sqlDataSync.SyncAllApprenticeships();

        [FunctionName(nameof(SyncCoursesCosmosCollectionToSql))]
        [NoAutomaticTrigger]
        public Task SyncCoursesCosmosCollectionToSql(string input) => _sqlDataSync.SyncAllCourses();

        [FunctionName(nameof(SyncUkrlpCosmosCollectionToSql))]
        [NoAutomaticTrigger]
        public Task SyncUkrlpCosmosCollectionToSql(string input) => _sqlDataSync.SyncAllProviders();

        [FunctionName(nameof(SyncVenuesCosmosCollectionToSql))]
        [NoAutomaticTrigger]
        public Task SyncVenuesCosmosCollectionToSql(string input) => _sqlDataSync.SyncAllVenues();
    }
}
