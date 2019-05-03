using Dfc.CourseDirectory.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Apprenticeships
{
    public interface IStandard
    {
        Guid id { get; set; } // Cosmos DB id

        // Standard specific properties. First two form composite primary key
        int StandardCode { get; set; }
        int Version { get; set; }

        string StandardName { get; set; }
        string StandardSectorCode { get; set; } // FK - For backwards compatibility with Tribal
        Guid StandardSectorCodeId { get; set; } // FK - CD  Cosmos DB ID
        DateTime? EffectiveFrom { get; set; }
        DateTime? EffectiveTo { get; set; }
        string URLLink { get; set; }
        decimal? SectorSubjectAreaTier1 { get; set; }
        Guid? SectorSubjectAreaTier1Id { get; set; } // ??
        decimal? SectorSubjectAreaTier2 { get; set; }
        Guid? SectorSubjectAreaTier2Id { get; set; } // ??
        string NotionalEndLevel { get; set; }
        string OtherBodyApprovalRequired { get; set; }

        // Standard auditing properties 
        RecordStatus RecordStatus { get; set; }
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
        DateTime? UpdatedDate { get; set; }
        string UpdatedBy { get; set; }
    }
}
