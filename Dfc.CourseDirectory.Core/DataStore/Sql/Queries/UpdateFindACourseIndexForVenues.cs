using System;
using System.Collections.Generic;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpdateFindACourseIndexForVenues : ISqlQuery<Success>
    {
        public IEnumerable<Guid> VenueIds { get; set; }
        public DateTime Now { get; set; }
    }
}
