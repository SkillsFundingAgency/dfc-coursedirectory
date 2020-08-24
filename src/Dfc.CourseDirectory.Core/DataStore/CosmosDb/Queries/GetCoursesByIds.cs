using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class GetCoursesByIds : ICosmosDbQuery<IDictionary<Guid, Course>>
    {
        public int Ukprn { get; set; }
        public IEnumerable<Guid> CourseIds { get; set; }
    }
}
