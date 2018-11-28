using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services
{
    public class VenueSearchResult : ValueObject<VenueSearchResult>, IVenueSearchResult
    {
        
        public IEnumerable<VenueSearchResultItem> Value { get; set; }

        public VenueSearchResult(
            IEnumerable<VenueSearchResultItem> value)
        {
            Throw.IfNull(value, nameof(value));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
