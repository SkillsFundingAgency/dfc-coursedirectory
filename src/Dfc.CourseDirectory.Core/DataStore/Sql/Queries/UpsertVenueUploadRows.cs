using System;
using System.Collections.Generic;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpsertVenueUploadRows : ISqlQuery<Success>
    {
        public Guid VenueUploadId { get; set; }
        public DateTime CreatedOn { get; set; }
        public IEnumerable<UpsertVenueUploadRowsRecord> Records { get; set; }
    }

    public class UpsertVenueUploadRowsRecord
    {
        public int RowNumber { get; set; }
        public bool IsValid { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public string ProviderVenueRef { get; set; }
        public string VenueName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Website { get; set; }
    }
}
