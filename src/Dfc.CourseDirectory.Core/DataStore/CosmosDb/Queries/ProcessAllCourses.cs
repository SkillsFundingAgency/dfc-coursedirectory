using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class ProcessAllCourses : ICosmosDbQuery<None>
    {
        public Expression<Func<Course, bool>> Filter { get; set; }
        public Func<IReadOnlyCollection<Course>, Task> ProcessChunk { get; set; }
    }
}
