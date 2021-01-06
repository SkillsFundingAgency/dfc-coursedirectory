using System;
using System.Collections.Generic;
using System.Text;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Models;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Models
{
    public class FeChoice : IFeChoice
    {
        public Guid Id { get; set; }
        public int UKPRN { get; set; }
        public double? LearnerSatisfaction { get; set; }
        public double? EmployerSatisfaction { get; set; }
        public DateTime? CreatedDateTimeUtc { get; set; }
    }
}
