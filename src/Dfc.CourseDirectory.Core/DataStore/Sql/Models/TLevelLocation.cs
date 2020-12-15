using System;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class TLevelLocation
    {
        public Guid TLevelLocationId { get; set; }
        public TLevelLocationStatus TLevelLocationStatus { get; set; }
        public Guid VenueId { get; set; }
        public string VenueName { get; set; }
    }
}
