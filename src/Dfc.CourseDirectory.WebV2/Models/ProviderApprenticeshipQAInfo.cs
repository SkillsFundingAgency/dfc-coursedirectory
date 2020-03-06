using System;

namespace Dfc.CourseDirectory.WebV2.Models
{
    public class ProviderApprenticeshipQAInfo
    {
        public Guid ProviderId { get; set; }
        public string ProviderName { get; set; }
        public int ProviderUkprn { get; set; }
        public ApprenticeshipQAStatus ApprenticeshipQAStatus { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public UserInfo LastAssessedBy { get; set; }
    }
}
