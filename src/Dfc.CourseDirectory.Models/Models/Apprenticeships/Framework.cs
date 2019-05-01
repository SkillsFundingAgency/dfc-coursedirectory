using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Interfaces.Apprenticeships;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.Apprenticeships
{
    public class Framework : IFramework
    {
        public Guid id { get; set; } // Cosmos DB id

        // Framework specific properties. First three form composite primary key
        public int FrameworkCode { get; set; } 
        public int ProgType { get; set; } // FK
        public Guid ProgTypeId { get; set; } // ???
        public int PathwayCode { get; set; }

        public string PathwayName { get; set; }
        public string NasTitle { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public decimal? SectorSubjectAreaTier1 { get; set; } // FK
        public Guid? SectorSubjectAreaTier1Id { get; set; } // ??
        public decimal? SectorSubjectAreaTier2 { get; set; } // FK
        public Guid? SectorSubjectAreaTier2Id { get; set; } // ??

        // Standard auditing properties 
        public RecordStatus RecordStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
