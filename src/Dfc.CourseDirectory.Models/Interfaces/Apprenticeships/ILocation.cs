using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Apprenticeships;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interface.Apprenticeships
{
    public interface ILocation
    {
        Guid id { get; set; } // Cosmos DB id

        Address Address { get; set; }

        int? ID { get; set; }

        string Name { get; set; }

        string Phone { get; set; }

        LocationType LocationType { get; set; }
        int ProviderUKPRN { get; set; } // As we are trying to inforce unique UKPRN per Provider

        int LocationId { get; set; }
        int ProviderId { get; set; }

        // Standard auditing properties 
        RecordStatus RecordStatus { get; set; }
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
        DateTime? UpdatedDate { get; set; }
        string UpdatedBy { get; set; }
    }
}
