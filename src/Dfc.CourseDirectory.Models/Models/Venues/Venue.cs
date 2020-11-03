using System;
using Dfc.CourseDirectory.Models.Interfaces.Venues;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.Models.Models.Venues
{
    public enum VenueStatus
    {
        Undefined = 0,
        Live = 1,
        Deleted = 2,
        Pending = 3,
        Uknown = 99
    }

    public class Venue : IVenue
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ID { get; }
        public int UKPRN { get; }
        [JsonProperty("PROVIDER_ID", Required = Required.AllowNull)]
        [JsonIgnore]
        public int? ProviderID { get; }
        [JsonProperty("VENUE_ID", Required = Required.AllowNull)]
        [JsonIgnore]
        public int VenueID { get; }
        [JsonProperty("VENUE_NAME")]
        public string VenueName { get; set; }
        [JsonProperty("PROV_VENUE_ID", Required = Required.AllowNull)]
        [JsonIgnore]
        public string ProvVenueID { get; }
        [JsonProperty("ADDRESS_1")]
        public string Address1 { get; }
        [JsonProperty("ADDRESS_2")]
        public string Address2 { get; }
        [JsonProperty("ADDRESS_3")]
        public string Address3 { get; }
        [JsonProperty("TOWN")]
        public string Town { get; }
        [JsonProperty("COUNTY")]
        public string County { get; }
        [JsonProperty("POSTCODE")]
        public string PostCode { get; }
        [JsonProperty("LATITUDE")]
        public decimal Latitude { get; set; }
        [JsonProperty("LONGITUDE")]
        public decimal Longitude { get; set; }
        public VenueStatus Status { get; set; }
        public DateTime DateAdded { get; }
        public DateTime DateUpdated { get; }
        public string UpdatedBy { get; }

        // Apprenticeship related
        public int? LocationId { get; set; }
        public int? TribalLocationId { get; set; }
        [JsonProperty("PHONE")]
        public string Telephone { get; set; }
        [JsonProperty("EMAIL")]
        public string Email { get; set; }
        [JsonProperty("WEBSITE")]
        public string Website { get; set; }

        public Venue(
            string id,
            int ukPrn,
            string venueName,
            string address1,
            string address2,
            string address3,
            string town,
            string county,
            string postcode,
            //string telephone,
            //string email,
            //string website,
            decimal latitude,
            decimal longitude,
            VenueStatus status,
            string updatedBy,
            DateTime dateAdded,
            DateTime dateUpdated)
        {
            if (ukPrn < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ukPrn), $"{nameof(ukPrn)} cannot be less than 0.");
            }

            if (string.IsNullOrWhiteSpace(venueName))
            {
                throw new ArgumentException("message", nameof(venueName));
            }

            if (string.IsNullOrWhiteSpace(address1))
            {
                throw new ArgumentException("message", nameof(address1));
            }

            if (string.IsNullOrWhiteSpace(town))
            {
                throw new ArgumentException("message", nameof(town));
            }

            if (string.IsNullOrWhiteSpace(postcode))
            {
                throw new ArgumentException("message", nameof(postcode));
            }

            ID = id;
            UKPRN = ukPrn;
            VenueName = venueName;
            Address1 = address1;
            Address2 = address2;
            Address3 = address3;
            Town = town;
            County = county;
            PostCode = postcode;
            //Telephone = telephone;
            //Email = email;
            //Website = website;
            Latitude = latitude;
            Longitude = longitude;
            Status = status;
            UpdatedBy = updatedBy;
            DateAdded = dateAdded;
            DateUpdated = dateUpdated;
        }


        [JsonConstructor]
        public Venue(
            string id,
            int ukPrn,
            int providerID,
            int venueID,
            string venueName,
            string provVenueID,
            string address1,
            string address2,
            string address3,
            string town,
            string county,
            string postcode,
            string phone,
            string email,
            string website,
            decimal latitude,
            decimal longitude,
            VenueStatus status,
            string updatedBy,
            DateTime dateAdded,
            DateTime dateUpdated)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("message", nameof(id));
            }

            if (ukPrn < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ukPrn), $"{nameof(ukPrn)} cannot be less than 0.");
            }

            ID = id;
            UKPRN = ukPrn;
            ProviderID = providerID;
            VenueID = venueID;
            VenueName = venueName;
            ProvVenueID = provVenueID;
            Address1 = address1;
            Address2 = address2;
            Address3 = address3;
            Town = town;
            County = county;
            PostCode = postcode;
            Telephone = phone;
            Email = email;
            Website = website;
            Latitude = latitude;
            Longitude = longitude;
            Status = status;
            UpdatedBy = updatedBy;
            DateAdded = dateAdded;
            DateUpdated = dateUpdated;
        }


        public Venue(
            string id,
            int ukPrn,
            string venueName,
            string address1,
            string address2,
            string address3,
            string town,
            string county,
            string postcode,
            decimal latitude,
            decimal longitude,
            VenueStatus status,
            string updatedBy,
            DateTime dateUpdated)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("message", nameof(id));
            }

            if (ukPrn < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ukPrn), $"{nameof(ukPrn)} cannot be less than 0.");
            }

            ID = id;
            UKPRN = ukPrn;
            VenueName = venueName;
            Address1 = address1;
            Address2 = address2;
            Address3 = address3;
            Town = town;
            County = county;
            PostCode = postcode;
            Latitude = latitude;
            Longitude = longitude;
            Status = status;
            UpdatedBy = updatedBy;
            DateUpdated = dateUpdated;
        }

        public Venue() { }
    }
}
