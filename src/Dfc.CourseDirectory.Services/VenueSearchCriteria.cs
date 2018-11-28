using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services
{
    public class VenueSearchCriteria : ValueObject<VenueSearchCriteria>, IVenueSearchCriteria
    {
        public string Search { get; }

        public VenueSearchCriteria(
            string search)
        {
            Throw.IfNullOrWhiteSpace(search, nameof(search));

            Search = search;
        }
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Search;
        }
    }
}
