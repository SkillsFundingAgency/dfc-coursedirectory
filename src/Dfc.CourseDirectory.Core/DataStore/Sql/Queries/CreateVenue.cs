using System;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class CreateVenue : ISqlQuery<Success>
    {
        public Guid VenueId { get; set; }
        public int ProviderUkprn { get; set; }
        public string Name { get; set; }
        public string ProviderVenueRef { get; set; }
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
        public DateTime CreatedOn { get; set; }
        public UserInfo CreatedBy { get; set; }
    }
}
