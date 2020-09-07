using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class GetAllCoursesForProvider : ICosmosDbQuery<IReadOnlyCollection<Course>>
    {
        public int ProviderUkprn { get; set; }
        public CourseStatus CourseStatuses { get; set; }
    }
}
