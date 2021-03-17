using System;

namespace Dfc.CourseDirectory.Services.VenueService
{
    public class VenueSearchCriteria
    {
        public string Search { get; }

        public VenueSearchCriteria(
            string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                throw new ArgumentNullException($"{nameof(search)} cannot be null or empty or whitespace.", nameof(search));
            }

            Search = search;
        }
    }
}
