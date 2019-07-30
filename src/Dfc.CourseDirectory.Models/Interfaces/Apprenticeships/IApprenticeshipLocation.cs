using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Apprenticeships;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Apprenticeships
{
    public interface IApprenticeshipLocation
    {
        Guid Id { get; set; } // Cosmos DB id
        Guid VenueId { get; set; }
        int? TribalId { get; set; }
        int ApprenticeshipLocationId { get; set; }
        Guid? LocationGuidId { get; set; }
        int? LocationId { get; set; }
        bool? National { get; set; }
        Address Address { get; set; }
        List<int> DeliveryModes { get; set; }
        string Name { get; set; }
        string Phone { get; set; }
        int ProviderUKPRN { get; set; } // As we are trying to inforce unique UKPRN per Provider
        int ProviderId { get; set; }
        string[] Regions { get; set; }
        ApprenticeshipLocationType ApprenticeshipLocationType { get; set; }
        LocationType LocationType { get; set; }
        int? Radius { get; set; }
        // Standard auditing properties 
        RecordStatus RecordStatus { get; set; }
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
        DateTime? UpdatedDate { get; set; }
        string UpdatedBy { get; set; }
    }
}
