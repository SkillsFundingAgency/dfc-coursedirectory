using Dfc.CourseDirectory.Common;
using System.Collections.Generic;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.Services.VenueService
{
    public class UpdatedVenueResult : ValueObject<UpdatedVenueResult>, IUpdatedVenueResult
    {
        [JsonProperty("ID")]
        public string Id { get; }

        [JsonProperty("VENUE_NAME")]
        public string VenueName { get; }
        //public string ProvVenueID { get; }
        [JsonProperty("ADDRESS_1")]
        public string Address1 { get; }

        [JsonProperty("ADDRESS_2")]
        public string Address2 { get; }
        public string Address3 { get; }

        [JsonProperty("TOWN")]
        public string Town { get; }

        [JsonProperty("COUNTY")]
        public string County { get; }

        [JsonProperty("POSTCODE")]
        public string PostCode { get; }

        public UpdatedVenueResult(
            string id,
            string venueName,
            string address1,
            string address2,
            string address3,
            string town,
            string county,
            string postcode)
        {
            Throw.IfNullOrWhiteSpace(id, nameof(Id));
            Throw.IfNullOrWhiteSpace(venueName, nameof(venueName));
            Throw.IfNullOrWhiteSpace(address1, nameof(address1));
            Throw.IfNullOrWhiteSpace(address1, nameof(address2));
            Throw.IfNullOrWhiteSpace(address1, nameof(address3));
            Throw.IfNullOrWhiteSpace(town, nameof(town));
            Throw.IfNullOrWhiteSpace(postcode, nameof(postcode));

            Id= id;
            VenueName = venueName;
            Address1 = address1;
            Address2 = address2;
            Address3 = address3;
            Town = town;
            County = county;
            PostCode = postcode;

        }
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
            yield return VenueName;
            yield return Address1;
            yield return Address2;
            yield return Address3;
            yield return Town;
            yield return County;
            yield return PostCode;
        }
    }
}
