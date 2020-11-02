using System.Collections.Generic;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Venues;
using Dfc.CourseDirectory.Services.Interfaces;

namespace Dfc.CourseDirectory.Services.VenueService
{
    public class VenueSearchResult : IVenueSearchResult
    {
        
        public IEnumerable<Venue> Value { get; set; }

        public VenueSearchResult(
            IEnumerable<Venue> value)
        {
            Throw.IfNull(value, nameof(value));
            Value = value;
        }
    }
}
