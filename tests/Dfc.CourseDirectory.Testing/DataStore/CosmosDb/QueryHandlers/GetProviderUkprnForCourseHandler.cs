using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class GetProviderUkprnForCourseHandler : ICosmosDbQueryHandler<GetProviderUkprnForCourse, int?>
    {
        public int? Execute(InMemoryDocumentStore inMemoryDocumentStore, GetProviderUkprnForCourse request)
        {
            var course = inMemoryDocumentStore.Courses.All
                .SingleOrDefault(c => c.Id == request.CourseId &&
                    c.CourseStatus != CourseStatus.Archived &&
                    c.CourseStatus != CourseStatus.Deleted);

            return course?.ProviderUKPRN;
        }
    }
}
