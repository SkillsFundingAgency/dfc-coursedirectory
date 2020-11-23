using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpsertVenuesFromCosmos : ISqlQuery<None>
    {
        public IEnumerable<UpsertVenuesRecord> Records { get; set; }
        public DateTime LastSyncedFromCosmos { get; set; }
    }

    public class UpsertVenuesRecord
    {
        public Guid VenueId { get; set; }
        public VenueStatus VenueStatus { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public string VenueName { get; set; }
        public int ProviderUkprn { get; set; }
        public int? TribalVenueId { get; set; }
        public string ProviderVenueRef { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public (double Latitude, double Longitude) Position { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
    }
}
