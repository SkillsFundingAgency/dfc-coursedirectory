using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Models.Enums;

namespace Dfc.CourseDirectory.Models.Models.Apprenticeships
{
    public class Framework
    {
        public Contact Contact { get; set; }
        public int? Level { get; set; }
        public List<LocationRef> Locations { get; set; }
        public Guid id { get; set; }
        public string FrameworkInfoUrl { get; set; }
        public string MarketingInfo { get; set; }
        public int FrameworkCode { get; set; } 
        public int ProgType { get; set; }
        public Guid ProgTypeId { get; set; }
        public int PathwayCode { get; set; }
        public string PathwayName { get; set; }
        public string NasTitle { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public decimal? SectorSubjectAreaTier1 { get; set; }
        public Guid? SectorSubjectAreaTier1Id { get; set; }
        public decimal? SectorSubjectAreaTier2 { get; set; }
        public Guid? SectorSubjectAreaTier2Id { get; set; }
        public RecordStatus RecordStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
