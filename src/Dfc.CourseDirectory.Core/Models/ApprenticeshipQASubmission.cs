using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Core.Models
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
        public DateTime? LastAssessedOn { get; set; }
        public bool? ProviderAssessmentPassed { get; set; }
        public bool? ApprenticeshipAssessmentsPassed { get; set; }
        public IReadOnlyCollection<ApprenticeshipQASubmissionApprenticeship> Apprenticeships { get; set; }
        public bool HidePassedNotification { get; set; }
    }

    public class ApprenticeshipQASubmissionApprenticeship
    {
        public int ApprenticeshipQASubmissionApprenticeshipId { get; set; }
        public Guid ApprenticeshipId { get; set; }
        public string ApprenticeshipTitle { get; set; }
        public string ApprenticeshipMarketingInformation { get; set; }
    }
}
