using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services
{
    public class VenueAdd : ValueObject<VenueAdd>, IVenueAdd
    {
        public string ADDRESS_1 { get; }
        public string ADDRESS_2 { get; }

        public string TOWN { get; }

        public string VENUE_NAME { get; }

        public string COUNTY { get; }

        public string POSTCODE { get; }

        public string UKPRN { get; }

        public VenueAdd(
            string address_1,
            string address_2,
            string town,
            string venue_name,
            string county,
            string postcode,
            string ukprn)
        {
            Throw.IfNullOrWhiteSpace(address_1, nameof(address_1));
            //Throw.IfNullOrWhiteSpace(address_2, nameof(address_2));
            Throw.IfNullOrWhiteSpace(town, nameof(town));
            Throw.IfNullOrWhiteSpace(venue_name, nameof(venue_name));
            Throw.IfNullOrWhiteSpace(county, nameof(county));
            Throw.IfNullOrWhiteSpace(postcode, nameof(postcode));
            Throw.IfNullOrWhiteSpace(ukprn, nameof(ukprn));

            ADDRESS_1 = address_1;
            ADDRESS_2 = address_2;
            TOWN = town;
            VENUE_NAME = venue_name;
            COUNTY = county;
            POSTCODE = postcode;
            UKPRN= ukprn;

        }
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return ADDRESS_1;
            yield return ADDRESS_2;
            yield return TOWN;
            yield return VENUE_NAME;
            yield return COUNTY;
            yield return POSTCODE;
            yield return UKPRN;
        }
    }
}
