using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Dfc.CourseDirectory.Models.Models.Venues;

namespace Dfc.CourseDirectory.Services.VenueService
{
    public class VenueSearchResult : ValueObject<VenueSearchResult>, IVenueSearchResult
    {
        
        public IEnumerable<Venue> Value { get; set; }

        public VenueSearchResult(
            IEnumerable<Venue> value)
        {
            Throw.IfNull(value, nameof(value));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
