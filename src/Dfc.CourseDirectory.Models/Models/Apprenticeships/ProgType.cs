using System;
using Dfc.CourseDirectory.Models.Enums;

namespace Dfc.CourseDirectory.Models.Models.Apprenticeships
{
    public class ProgType
    {
        public Guid id { get; set; }
        public int ProgTypeId { get; set; }
        public string ProgTypeDesc { get; set; }
        public string ProgTypeDesc2 { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public RecordStatus RecordStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
