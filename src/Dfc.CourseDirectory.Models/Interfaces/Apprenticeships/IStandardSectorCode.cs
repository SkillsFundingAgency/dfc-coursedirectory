using Dfc.CourseDirectory.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Apprenticeships
{
    public interface IStandardSectorCode
    {
        Guid id { get; set; } // Cosmos DB id

        string StandardSectorCodeId { get; set; }
        string StandardSectorCodeDesc { get; set; }
        string StandardSectorCodeDesc2 { get; set; }
        DateTime? EffectiveFrom { get; set; }
        DateTime? EffectiveTo { get; set; }

        // Standard auditing properties 
        RecordStatus RecordStatus { get; set; }
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
        DateTime? UpdatedDate { get; set; }
        string UpdatedBy { get; set; }
    }
}
