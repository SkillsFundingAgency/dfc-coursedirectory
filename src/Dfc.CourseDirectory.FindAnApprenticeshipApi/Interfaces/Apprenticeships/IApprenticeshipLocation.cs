using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Models;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Models.Enums;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Apprenticeships
{
    public interface IApprenticeshipLocation
    {
        Guid Id { get; } // Cosmos DB id
        bool? National { get; }
        Address Address { get; }
        List<int> DeliveryModes { get; }
        string Name { get; }
        string Phone { get; }
        int ProviderUKPRN { get; } // As we are trying to inforce unique UKPRN per Provider
        int? ProviderId { get; }
        string[] Regions { get; }
        ApprenticeshipLocationType ApprenticeshipLocationType { get; }
        LocationType LocationType { get; }
        int? Radius { get; }
        // Standard auditing properties 
        RecordStatus RecordStatus { get; }
        DateTime CreatedDate { get; }
        string CreatedBy { get; }
        DateTime? UpdatedDate { get; }
        string UpdatedBy { get; }
    }
}
