using System;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class CreateVenueUploadRow : ISqlQuery<int>
    {
        public int VenueUploadRowId { get; set; }
        public Guid VenueUploadId { get; set; }
        public int RowNumber { get; set; }
        public int VenueUploadRowStatus { get; set; }
        public bool IsValid { get; set; }
        public string Errors { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime LastValidated { get; set; }
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
        public Guid VenueId { get; set; }
        public bool OutsideOfEngland { get; set; }
        public bool IsSupplementary { get; set; }

    }
}
