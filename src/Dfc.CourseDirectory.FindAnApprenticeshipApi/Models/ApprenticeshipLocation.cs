using Dfc.Providerportal.FindAnApprenticeship.Interfaces.Apprenticeships;
using Dfc.Providerportal.FindAnApprenticeship.Models.Enums;
using Dfc.Providerportal.FindAnApprenticeship.Models.DAS;
using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.Providerportal.FindAnApprenticeship.Helper;

namespace Dfc.Providerportal.FindAnApprenticeship.Models
{
    public class ApprenticeshipLocation : IApprenticeshipLocation
    {
        public Guid Id { get; set; }
        public bool? National { get; set; }
        public Address Address { get; set; }
        public List<int> DeliveryModes { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public int ProviderUKPRN { get; set; } // As we are trying to inforce unique UKPRN per Provider
        public int? ProviderId { get; set; }
        public string[] Regions { get; set; }
        public ApprenticeshipLocationType ApprenticeshipLocationType { get; set; }
        public LocationType LocationType { get; set; }
        public int? Radius { get; set; }
        // Standard auditing properties 
        public RecordStatus RecordStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class ApprenticeshipLocationSameAddress : EqualityComparer<ApprenticeshipLocation>
    {
        public override bool Equals(ApprenticeshipLocation alpha, ApprenticeshipLocation beta)
        {
            if (alpha == null && beta == null)
                return true;
            else if (alpha == null || beta == null)
                return false;

            return alpha.ToAddressHash() == beta.ToAddressHash();
        }

        public override int GetHashCode(ApprenticeshipLocation app)
        {
            var stringToHash = app.ToAddressHash();
            return stringToHash;
        }
    }
}
