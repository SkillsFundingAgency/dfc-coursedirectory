using Dfc.CourseDirectory.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Apprenticeships
{
    public interface ISectorSubjectAreaTier2
    {
        Guid id { get; set; } // Cosmos DB id

        decimal SectorSubjectAreaTier2Id { get; set; }
        string SectorSubjectAreaTier2Desc { get; set; }
        string SectorSubjectAreaTier2Desc2 { get; set; }
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
