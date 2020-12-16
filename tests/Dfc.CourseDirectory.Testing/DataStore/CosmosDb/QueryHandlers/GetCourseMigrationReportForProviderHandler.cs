using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class GetCourseMigrationReportForProviderHandler : ICosmosDbQueryHandler<GetCourseMigrationReportForProvider, CourseMigrationReport>
    {
        public CourseMigrationReport Execute(InMemoryDocumentStore inMemoryDocumentStore, GetCourseMigrationReportForProvider request) =>
            inMemoryDocumentStore.CourseMigrationReports.All
                .FirstOrDefault(r => r.ProviderUKPRN == request.ProviderUkprn);
    }
}
