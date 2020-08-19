using System;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class GetProviderUkprnForCourse : ICosmosDbQuery<int?>
    {
        public Guid CourseId { get; set; }
    }
}
