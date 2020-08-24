using System;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class DeleteCourseRun : ICosmosDbQuery<OneOf<NotFound, Success>>
    {
        public Guid CourseId { get; set; }
        public Guid CourseRunId { get; set; }
        public int ProviderUkprn { get; set; }
    }
}
