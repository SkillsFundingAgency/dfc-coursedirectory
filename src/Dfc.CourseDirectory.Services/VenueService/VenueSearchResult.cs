using System;
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
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Value = value;
        }
    }
}
