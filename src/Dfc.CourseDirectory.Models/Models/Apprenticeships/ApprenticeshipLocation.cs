using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Interfaces.Apprenticeships;
using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Models.Models.Apprenticeships
{
    public class ApprenticeshipLocation : IApprenticeshipLocation
    {
        public Guid Id { get; set; }
        public Guid VenueId { get; set; }
        public int? TribalId { get; set; }
        public int ApprenticeshipLocationId { get; set; }
        public Guid? LocationGuidId { get; set; }
        public int? LocationId { get; set; }
        public bool? National { get; set; }
        public Address Address { get; set; }
        public List<int> DeliveryModes { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public int ProviderUKPRN { get; set; } // As we are trying to inforce unique UKPRN per Provider
        public int ProviderId { get; set; }
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
}
