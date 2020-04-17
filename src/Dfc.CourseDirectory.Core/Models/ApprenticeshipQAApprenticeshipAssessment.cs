using System;

namespace Dfc.CourseDirectory.Core.Models
{
    public class ApprenticeshipQAApprenticeshipAssessment
    {
        public int ApprenticeshipQASubmissionId { get; set; }
        public DateTime AssessedOn { get; set; }
        public UserInfo AssessedBy { get; set; }
        public bool Passed { get; set; }
        public bool? CompliancePassed { get; set; }
        public ApprenticeshipQAApprenticeshipComplianceFailedReasons ComplianceFailedReasons { get; set; }
        public string ComplianceComments { get; set; }
        public bool? StylePassed { get; set; }
        public ApprenticeshipQAApprenticeshipStyleFailedReasons StyleFailedReasons { get; set; }
        public string StyleComments { get; set; }
    }
}
