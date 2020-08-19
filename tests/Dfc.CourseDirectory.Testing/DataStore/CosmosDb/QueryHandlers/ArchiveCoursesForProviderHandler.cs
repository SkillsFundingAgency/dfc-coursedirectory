using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class ArchiveCoursesForProviderHandler : ICosmosDbQueryHandler<ArchiveCoursesForProvider, int>
    {
        public int Execute(InMemoryDocumentStore inMemoryDocumentStore, ArchiveCoursesForProvider request)
        {
            var providerCourses = inMemoryDocumentStore.Courses.All
                .Where(a => a.ProviderUKPRN == request.Ukprn && a.CourseStatus != CourseStatus.Archived);

            var updated = 0;

            foreach (var app in providerCourses)
            {
                app.CourseStatus = CourseStatus.Archived;
                
                foreach (var courseRun in app.CourseRuns)
                {
                    courseRun.RecordStatus = CourseStatus.Archived;
                }

                inMemoryDocumentStore.Courses.Save(app);

                updated++;
            }

            return updated;
        }
    }
}
