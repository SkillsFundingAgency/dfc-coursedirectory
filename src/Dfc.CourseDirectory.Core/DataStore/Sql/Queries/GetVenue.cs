using System;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetVenue : ISqlQuery<Venue>
    {
        public Guid VenueId { get; set; }
    }
}
