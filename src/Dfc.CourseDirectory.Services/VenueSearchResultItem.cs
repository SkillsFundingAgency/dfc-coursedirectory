using Dfc.CourseDirectory.Common;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services
{
    public class VenueSearchResultItem : ValueObject<VenueSearchResultItem>, IVenueSearchResultItem
    {
        public string ID { get; }
        public int UKPRN { get; }
        public int ProviderID { get; }
        public int VenueID { get; }
        public string VenueName { get; }
        public string ProvVenueID { get; }
        public string Address1 { get; }
        public string Address2 { get; }
        public string Address3 { get; }
        public string Town { get; }
        public string County { get; }
        public string PostCode { get; }

        public VenueSearchResultItem(
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
            string postcode)
        {
            Throw.IfNullOrWhiteSpace(ID, nameof(ID));
            Throw.IfLessThan(0, ukPrn, nameof(ukPrn));
            Throw.IfLessThan(0, providerID, nameof(providerID));
            Throw.IfLessThan(0, venueID, nameof(venueID));
            Throw.IfNullOrWhiteSpace(venueName, nameof(venueName));
            Throw.IfNullOrWhiteSpace(provVenueID, nameof(provVenueID));
            Throw.IfNullOrWhiteSpace(address1, nameof(address1));
            Throw.IfNullOrWhiteSpace(address1, nameof(address2));
            Throw.IfNullOrWhiteSpace(address1, nameof(address3));
            Throw.IfNullOrWhiteSpace(town, nameof(town));
            Throw.IfNullOrWhiteSpace(postcode, nameof(postcode));

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
        }
    }
}
