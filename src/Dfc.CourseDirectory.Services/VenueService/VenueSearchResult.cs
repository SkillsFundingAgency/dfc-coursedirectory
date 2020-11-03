using System.Collections.Generic;
using Dfc.CourseDirectory.Services.Models.Venues;

namespace Dfc.CourseDirectory.Services.VenueService
{
    public class VenueSearchResult
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
