using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class ProcessAllCourses : ICosmosDbQuery<None>
    {
        public Func<IReadOnlyCollection<Course>, Task> ProcessChunk { get; set; }
    }
}
