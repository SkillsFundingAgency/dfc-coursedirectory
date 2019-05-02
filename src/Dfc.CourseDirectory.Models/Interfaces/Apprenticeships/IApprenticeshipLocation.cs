using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Apprenticeships;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Apprenticeships
{
    public interface IApprenticeshipLocation
    {
        Guid id { get; set; } // Cosmos DB id

        Guid LocationId { get; set; }
        int? TribalLocationId { get; set; }
        List<string> DeliveryModes { get; set; }

        ApprenticeshipLocationType ApprenticeshipLocationType { get; set; }

        int? Radius { get; set; }

        // Standard auditing properties 
        RecordStatus RecordStatus { get; set; }
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
        DateTime? UpdatedDate { get; set; }
        string UpdatedBy { get; set; }
    }
}
