using System;
using Dfc.CourseDirectory.Services.Models;

namespace Dfc.CourseDirectory.Services.Models.Apprenticeships
{
    public class SectorSubjectAreaTier1
    {
        public Guid id { get; set; }
        public decimal SectorSubjectAreaTier1Id { get; set; }
        public string SectorSubjectAreaTier1Desc { get; set; }
        public string SectorSubjectAreaTier1Desc2 { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public RecordStatus RecordStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
