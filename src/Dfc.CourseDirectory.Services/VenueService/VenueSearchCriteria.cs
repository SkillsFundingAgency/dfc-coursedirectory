namespace Dfc.CourseDirectory.Services.VenueService
{
    public class VenueSearchCriteria
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
