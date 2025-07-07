using System;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataManagement.Schemas
{
    public class CsvVenueRow
    {
        private string _addressLine2;
        private string _county;
        private string _postcode;
        private string _email;
        private string _telephone;

        [Index(0), Name("YOUR_VENUE_REFERENCE")]
        public string ProviderVenueRef { get; set; }

        [Index(1), Name("VENUE_NAME")]
        public string VenueName { get; set; }

        [Index(2), Name("ADDRESS_LINE_1")]
        public string AddressLine1 { get; set; }

        [Index(3), Name("ADDRESS_LINE_2")]
        public string AddressLine2
        {
            get => string.IsNullOrWhiteSpace(_addressLine2) ? null : _addressLine2;
            set => _addressLine2 = value;
        }

        [Index(4), Name("TOWN_OR_CITY")]
        public string Town { get; set; }

        [Index(5), Name("COUNTY")]
        public string County
        {
            get => string.IsNullOrWhiteSpace(_county) ? null : _county;
            set => _county = value;
        }

        [Index(6), Name("POSTCODE")]
        public string Postcode
        {
            get => string.IsNullOrWhiteSpace(_postcode) ? null : _postcode;
            set => _postcode = value;
        }

        [Index(7), Name("EMAIL")]
        public string Email
        {
            get => string.IsNullOrWhiteSpace(_email) ? null : _email;
            set => _email = value;
        }

        [Index(8), Name("PHONE")]
        public string Telephone
        {
            get => string.IsNullOrWhiteSpace(_telephone) ? null : _telephone;
            set => _telephone = value;
        }

        [Index(9), Name("WEBSITE")]
        public string Website { get; set; }

        public static CsvVenueRow FromModel(Venue venue) => new CsvVenueRow()
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

        public static CsvVenueRow FromModel(VenueUploadRow row) => new CsvVenueRow()
        {
            ProviderVenueRef = row.ProviderVenueRef,
            VenueName = row.VenueName,
            AddressLine1 = row.AddressLine1,
            AddressLine2 = row.AddressLine2,
            Town = row.Town,
            County = row.County,
            Postcode = row.Postcode,
            Email = row.Email,
            Telephone = row.Telephone,
            Website = row.Website
        };        
    }
}
