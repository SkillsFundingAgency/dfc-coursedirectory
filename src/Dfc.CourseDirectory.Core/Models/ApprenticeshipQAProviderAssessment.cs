using System;

namespace Dfc.CourseDirectory.Core.Models
{
    public class ApprenticeshipQAProviderAssessment
    {
        public int ApprenticeshipQASubmissionId { get; set; }
        public DateTime AssessedOn { get; set; }
        public UserInfo AssessedBy { get; set; }
        public bool Passed { get; set; }
        public bool? CompliancePassed { get; set; }
        public ApprenticeshipQAProviderComplianceFailedReasons ComplianceFailedReasons { get; set; }
        public string ComplianceComments { get; set; }
        public bool? StylePassed { get; set; }
        public ApprenticeshipQAProviderStyleFailedReasons StyleFailedReasons { get; set; }
        public string StyleComments { get; set; }
    }
}
