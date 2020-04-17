using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider
{
    public class QANotificationsViewModel
    {
        public ApprenticeshipQAStatus Status { get; set; }
        public ProviderType ProviderType { get; set; }
        public bool HidePassedNotication { get; set; }
    }
}
