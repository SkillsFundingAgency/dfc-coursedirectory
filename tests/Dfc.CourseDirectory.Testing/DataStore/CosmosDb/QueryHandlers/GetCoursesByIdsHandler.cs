using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class GetCoursesByIdsHandler
        : ICosmosDbQueryHandler<GetCoursesByIds, IDictionary<Guid, Course>>
    {
        public IDictionary<Guid, Course> Execute(InMemoryDocumentStore inMemoryDocumentStore, GetCoursesByIds request) =>
            inMemoryDocumentStore.Courses.All
                .Where(p => request.CourseIds.Contains(p.Id))
                .ToDictionary(p => p.Id, p => p);
    }
}
