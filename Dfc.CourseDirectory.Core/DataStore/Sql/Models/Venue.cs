using System;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class Venue
    {
        public Guid VenueId { get; set; }
        public Guid ProviderId { get; set; }
        public int ProviderUkprn { get; set; }
        public string VenueName { get; set; }
        public string ProviderVenueRef { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
