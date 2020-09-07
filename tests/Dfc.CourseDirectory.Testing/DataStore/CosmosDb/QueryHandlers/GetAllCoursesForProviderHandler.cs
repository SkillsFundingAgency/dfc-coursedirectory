using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class GetAllCoursesForProviderHandler :
        ICosmosDbQueryHandler<GetAllCoursesForProvider, IReadOnlyCollection<Course>>
    {
        public IReadOnlyCollection<Course> Execute(
            InMemoryDocumentStore inMemoryDocumentStore,
            GetAllCoursesForProvider request)
        {
            return inMemoryDocumentStore.Courses.All
                .Where(c => c.ProviderUKPRN == request.ProviderUkprn)
                .Where(c => (c.CourseStatus & request.CourseStatuses) != 0)
                .ToList();
        }
    }
}
