using System;
using FormFlow;

namespace Dfc.CourseDirectory.WebV2.Features.Venues.AddVenue
{
    [JourneyState]
    public class AddVenueJourneyModel
    {
        public AddVenueCompletedStages ValidStages { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Website { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public bool AddressIsOutsideOfEngland { get; set; }
        public Guid? VenueId { get; set; }
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
