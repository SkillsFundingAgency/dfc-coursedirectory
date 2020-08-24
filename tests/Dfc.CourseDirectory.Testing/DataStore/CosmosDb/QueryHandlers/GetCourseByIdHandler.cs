using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Testing.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class GetCourseByIdHandler : ICosmosDbQueryHandler<GetCourseById, Course>
    {
        public Course Execute(InMemoryDocumentStore inMemoryDocumentStore, GetCourseById request)
        {
            return inMemoryDocumentStore.Courses.All.SingleOrDefault(c => c.Id == request.CourseId);
        }
    }
}
