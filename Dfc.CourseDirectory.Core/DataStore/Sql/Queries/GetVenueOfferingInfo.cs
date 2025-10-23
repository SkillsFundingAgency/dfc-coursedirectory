using System;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetVenueOfferingInfo : ISqlQuery<VenueOfferingInfo>
    {
        public Guid VenueId { get; set; }
    }
}
