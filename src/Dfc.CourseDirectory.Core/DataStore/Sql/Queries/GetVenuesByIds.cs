using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetVenuesByIds : ISqlQuery<IReadOnlyDictionary<Guid, Venue>>
    {
        public IEnumerable<Guid> VenueIds { get; set; }
    }
}
