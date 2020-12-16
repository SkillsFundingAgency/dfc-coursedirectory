using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class GetCourseMigrationReportForProvider : ICosmosDbQuery<CourseMigrationReport>
    {
        public int ProviderUkprn { get; set; }
    }
}
