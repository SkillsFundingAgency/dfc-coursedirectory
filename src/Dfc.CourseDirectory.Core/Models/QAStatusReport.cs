using System;

namespace Dfc.CourseDirectory.Core.Models
{
    public class QAStatusReport
    {
        public Guid ProviderId { get; set; }
        public string UKPRN { get; set; }
        public string ProviderName { get; set; }
        public string Email { get; set; }
        public bool PassedQA => QAStatus == ApprenticeshipQAStatus.Passed;
        public DateTime? PassedQAOn { get; set; }
        public bool FailedQA => QAStatus == ApprenticeshipQAStatus.Failed;
        public DateTime? FailedQAOn { get; set; }
        public bool? UnableToComplete => QAStatus == ApprenticeshipQAStatus.UnableToComplete;
        public DateTime? UnableToCompleteOn { get; set; }
        public ApprenticeshipQAUnableToCompleteReasons? UnableToCompleteReasons { get; set; }
        public string Notes { get; set; }
        public ApprenticeshipQAStatus? QAStatus { get; set; }

    }
}
