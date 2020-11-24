using System;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class GetCourseById : ICosmosDbQuery<Course>
    {
        public Guid CourseId { get; set; }
    }
}
