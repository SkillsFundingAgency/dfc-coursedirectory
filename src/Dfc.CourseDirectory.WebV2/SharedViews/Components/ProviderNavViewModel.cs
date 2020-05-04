using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.WebV2.SharedViews.Components
{
    public class ProviderNavViewModel
    {
        public ProviderInfo ProviderContext { get; set; }
        public ApprenticeshipQAStatus ApprenticeshipQAStatus { get; set; }
        public bool ApprenticeshipQAFeatureIsEnabled { get; set; }
    }
}
