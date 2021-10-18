using System;
using FormFlow;
using Dfc.CourseDirectory.WebV2.AddressSearch;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.WebV2.Features.Venues.AddVenue
{
    [JourneyState]
    public class AddVenueJourneyModel
    {
        public AddVenueCompletedStages ValidStages { get; set; }
        public string ProviderVenueRef { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Website { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool AddressIsOutsideOfEngland { get; set; }
        public Guid? VenueId { get; set; }
        public IReadOnlyCollection<PostcodeSearchResult> PostcodeSearchResults { get; set; }
        public string PostcodeSearchQuery { get; set; }
    }

    [Flags]
    public enum AddVenueCompletedStages
    {
        None = 0,
        Address = 1,
        Details = 2,
        All = Address | Details
    }
}
