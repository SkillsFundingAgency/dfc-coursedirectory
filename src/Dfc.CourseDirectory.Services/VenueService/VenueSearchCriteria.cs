using Dfc.CourseDirectory.Services.Interfaces;

namespace Dfc.CourseDirectory.Services.VenueService
{
    public class VenueSearchCriteria : IVenueSearchCriteria
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
    }
}
