using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services.VenueService
{
    public class VenueSearchCriteria : ValueObject<VenueSearchCriteria>, IVenueSearchCriteria
    {
        public string Search { get; }
        public string NewAddressId { get; }

        public VenueSearchCriteria(
            string search,
            string newAddressId)
        {
            Throw.IfNullOrWhiteSpace(search, nameof(search));

            Search = search;
            NewAddressId = newAddressId;
        }
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Search;
            yield return NewAddressId;
        }
    }
}
