using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Interfaces.Venues;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.Models.Models.Venues
{
    public enum VenueStatus
    {
        Live = 97,
        Archived = 98,
        Uknown = 99
    }

    public class Venue : ValueObject<Venue>, IVenue
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ID { get; }
        public int UKPRN { get; }
        [JsonProperty("PROVIDER_ID", Required = Required.AllowNull)]
        [JsonIgnore]
        public int ProviderID { get; }
        [JsonProperty("VENUE_ID", Required = Required.AllowNull)]
        [JsonIgnore]
        public int VenueID { get; }
        [JsonProperty("VENUE_NAME")]
        public string VenueName { get; }
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
        public VenueStatus Status { get; }
        public DateTime DateAdded { get; }
        public DateTime DateUpdated { get; }
        public string UpdatedBy { get; }

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
            VenueStatus status,
            string updatedBy,
            DateTime dateAdded,
            DateTime dateUpdated)
        {
            //Throw.IfNullOrWhiteSpace(id, nameof(id));
            Throw.IfLessThan(0, ukPrn, nameof(ukPrn));
            Throw.IfNullOrWhiteSpace(venueName, nameof(VenueName));
            Throw.IfNullOrWhiteSpace(address1, nameof(address1));
            Throw.IfNullOrWhiteSpace(town, nameof(town));
            Throw.IfNullOrWhiteSpace(postcode, nameof(postcode));

            ID = id;
            UKPRN = ukPrn;
            VenueName = venueName;
            Address1 = address1;
            Address2 = address2;
            Address3 = address3;
            Town = town;
            County = county;
            PostCode = postcode;
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
            VenueStatus status,
            string updatedBy,
            DateTime dateAdded,
            DateTime dateUpdated)
        {
            Throw.IfNullOrWhiteSpace(id, nameof(id));
            Throw.IfLessThan(0, ukPrn, nameof(ukPrn));
            //Throw.IfLessThan(0, providerID, nameof(providerID));
            //Throw.IfLessThan(0, venueID, nameof(venueID));
           //Throw.IfNullOrWhiteSpace(venueName, nameof(venueName));
            //Throw.IfNullOrWhiteSpace(provVenueID, nameof(provVenueID));
            ////Throw.IfNullOrWhiteSpace(address1, nameof(address1));
            //Throw.IfNullOrWhiteSpace(postcode, nameof(postcode));
            //Throw.IfNullOrWhiteSpace(updatedBy, nameof(updatedBy));


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
            VenueStatus status,
            string updatedBy,
            DateTime dateUpdated)
        {
            Throw.IfNullOrWhiteSpace(id, nameof(id));
            Throw.IfLessThan(0, ukPrn, nameof(ukPrn));
            //Throw.IfLessThan(0, providerID, nameof(providerID));
            //Throw.IfLessThan(0, venueID, nameof(venueID));
            //Throw.IfNullOrWhiteSpace(venueName, nameof(venueName));
            //Throw.IfNullOrWhiteSpace(provVenueID, nameof(provVenueID));
            //Throw.IfNullOrWhiteSpace(address1, nameof(address1));
            //Throw.IfNullOrWhiteSpace(address1, nameof(address2));
            //Throw.IfNullOrWhiteSpace(address1, nameof(address3));
            //Throw.IfNullOrWhiteSpace(town, nameof(town));
            //Throw.IfNullOrWhiteSpace(postcode, nameof(postcode));
            //Throw.IfNullOrWhiteSpace(updatedBy, nameof(updatedBy));


            ID = id;
            UKPRN = ukPrn;
            VenueName = venueName;
            Address1 = address1;
            Address2 = address2;
            Address3 = address3;
            Town = town;
            County = county;
            PostCode = postcode;
            Status = status;
            UpdatedBy = updatedBy;
            DateUpdated = dateUpdated;

        }


        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return ID;
            yield return UKPRN;
            yield return ProviderID;
            yield return VenueID;
            yield return VenueName;
            yield return ProvVenueID;
            yield return Address1;
            yield return Address2;
            yield return Address3;
            yield return Town;
            yield return County;
            yield return PostCode;
            yield return Status;
            yield return UpdatedBy;
            yield return DateAdded;
            yield return DateUpdated;
        }
    }
}
