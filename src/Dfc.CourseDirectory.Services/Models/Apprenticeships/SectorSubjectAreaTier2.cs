using System;
using Dfc.CourseDirectory.Services.Enums;

namespace Dfc.CourseDirectory.Services.Models.Apprenticeships
{
    public class SectorSubjectAreaTier2
    {
        public Guid id { get; set; }
        public decimal SectorSubjectAreaTier2Id { get; set; }
        public string SectorSubjectAreaTier2Desc { get; set; }
        public string SectorSubjectAreaTier2Desc2 { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public RecordStatus RecordStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
