using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Interfaces.Apprenticeships;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.Apprenticeships
{
    public class Standard : IStandard
    {
        public Guid id { get; set; } // Cosmos DB id

        // Standard specific properties. First two form composite primary key
        public int StandardCode { get; set; }
        public int Version { get; set; }

        public string StandardName { get; set; }
        public string StandardSectorCode { get; set; } // FK - For backwards compatibility with Tribal
        public Guid StandardSectorCodeId { get; set; } // FK - CD  Cosmos DB ID
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string URLLink { get; set; }
        public decimal? SectorSubjectAreaTier1 { get; set; }
        public Guid? SectorSubjectAreaTier1Id { get; set; } // ??
        public decimal? SectorSubjectAreaTier2 { get; set; }
        public Guid? SectorSubjectAreaTier2Id { get; set; } // ??
        public string NotionalEndLevel { get; set; }
        public string OtherBodyApprovalRequired { get; set; }

        // Standard auditing properties 
        public RecordStatus RecordStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
