using System;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataManagement.Schemas
{
    public class VenueRow
    {
        [Index(0), Name("YOUR_VENUE_REFERENCE")]
        public string ProviderVenueRef { get; set; }

        [Index(1), Name("VENUE_NAME")]
        public string VenueName { get; set; }

        [Index(2), Name("ADDRESS_LINE_1")]
        public string AddressLine1 { get; set; }

        [Index(3), Name("ADDRESS_LINE_2")]
        public string AddressLine2 { get; set; }

        [Index(4), Name("TOWN_OR_CITY")]
        public string Town { get; set; }

        [Index(5), Name("COUNTY")]
        public string County { get; set; }

        [Index(6), Name("POSTCODE")]
        public string Postcode { get; set; }

        [Index(7), Name("EMAIL")]
        public string Email { get; set; }

        [Index(8), Name("PHONE")]
        public string Telephone { get; set; }

        [Index(9), Name("WEBSITE")]
        public string Website { get; set; }

        public static VenueRow FromModel(Venue venue) => new VenueRow()
        {
            ProviderVenueRef = venue.ProviderVenueRef,
            VenueName = venue.VenueName,
            AddressLine1 = venue.AddressLine1,
            AddressLine2 = venue.AddressLine2,
            Town = venue.Town,
            County = venue.County,
            Postcode = venue.Postcode,
            Email = venue.Email,
            Telephone = venue.Telephone,
            Website = venue.Website
        };
    }
}
