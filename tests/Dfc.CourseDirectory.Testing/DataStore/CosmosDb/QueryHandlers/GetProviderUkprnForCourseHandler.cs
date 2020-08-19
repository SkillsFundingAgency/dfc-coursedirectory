using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class GetProviderUkprnForCourseHandler : ICosmosDbQueryHandler<GetProviderUkprnForCourse, int?>
    {
        public int? Execute(InMemoryDocumentStore inMemoryDocumentStore, GetProviderUkprnForCourse request)
        {
            var course = inMemoryDocumentStore.Courses.All.SingleOrDefault(c => c.Id == request.CourseId);
            return course?.ProviderUKPRN;
        }
    }
}
