using System;

namespace Dfc.CourseDirectory.WebV2.Models
{
    public class ApprenticeshipQASubmission
    {
        public int ApprenticeshipQASubmissionId { get; set; }
        public Guid ProviderId { get; set; }
        public DateTime SubmittedOn { get; set; }
        public UserInfo SubmittedByUser { get; set; }
        public string ProviderMarketingInformation { get; set; }
        public bool? Passed { get; set; }
        public UserInfo LastAssessedBy { get; set; }
        public bool? ProviderAssessmentPassed { get; set; }
        public bool? ApprenticeshipAssessmentsPassed { get; set; }
    }
}
