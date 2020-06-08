using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class ArchiveCoursesForProviderHandler : ICosmosDbQueryHandler<ArchiveCoursesForProvider, int>
    {
        public int Execute(InMemoryDocumentStore inMemoryDocumentStore, ArchiveCoursesForProvider request)
        {
            var providerCourses = inMemoryDocumentStore.Courses.All
                .Where(a => a.ProviderUKPRN == request.Ukprn && a.CourseStatus != 4);

            var updated = 0;

            foreach (var app in providerCourses)
            {
                app.CourseStatus = 4;
                
                foreach (var courseRun in app.CourseRuns)
                {
                    courseRun.RecordStatus = 4;
                }

                inMemoryDocumentStore.Courses.Save(app);

                updated++;
            }

            return updated;
        }
    }
}
