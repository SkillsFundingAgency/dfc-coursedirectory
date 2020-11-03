using System;
using Dfc.CourseDirectory.Services.Enums;

namespace Dfc.CourseDirectory.Services.Models.Apprenticeships
{
    public class FEChoice
    {
        public Guid id { get; set; }
        public int UPIN { get; set; }
        public double? LearnerDestination { get; set; }
        public double? LearnerSatisfaction { get; set; }
        public double? EmployerSatisfaction { get; set; }
        public RecordStatus RecordStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
