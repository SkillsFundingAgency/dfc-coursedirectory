﻿using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Apprenticeships;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Apprenticeships
{
    public interface IFramework
    {
        Guid id { get; set; } // Cosmos DB id
        Contact Contact { get; set; }
        int? Level { get; set; }

        List<LocationRef> Locations { get; set; }
        int FrameworkCode { get; set; } 
        int ProgType { get; set; } // FK
        Guid ProgTypeId { get; set; } // ???
        int PathwayCode { get; set; }

        string PathwayName { get; set; }
        string NasTitle { get; set; }
        DateTime? EffectiveFrom { get; set; }
        DateTime? EffectiveTo { get; set; }
        decimal? SectorSubjectAreaTier1 { get; set; } // FK
        Guid? SectorSubjectAreaTier1Id { get; set; } // ??
        decimal? SectorSubjectAreaTier2 { get; set; } // FK
        Guid? SectorSubjectAreaTier2Id { get; set; } // ??

        // Standard auditing properties 
        RecordStatus RecordStatus { get; set; }
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
        DateTime? UpdatedDate { get; set; }
        string UpdatedBy { get; set; }
    }
}
