using Dfc.CourseDirectory.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Apprenticeships
{
    public interface ISectorSubjectAreaTier1
    {
        Guid id { get; set; } // Cosmos DB id


        decimal SectorSubjectAreaTier1Id { get; set; }
        string SectorSubjectAreaTier1Desc { get; set; }
        string SectorSubjectAreaTier1Desc2 { get; set; }
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
