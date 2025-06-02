using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Middleware;

namespace Dfc.CourseDirectory.WebV2.SharedViews.Components
{
    public class ProviderNavViewModel
    {
        public ProviderInfo ProviderInfo { get; set; }
        public bool ShowCoursesLinks { get; set; }
        public bool ShowTLevelsLinks { get; set; }
        public bool ShowLinksApartFromHomeAndSignOut { get; set; }

        public static ProviderNavViewModel Create(
            ProviderInfo providerInfo)
        {
            return new ProviderNavViewModel()
            {
                ProviderInfo = providerInfo,
                ShowCoursesLinks = providerInfo.ProviderType.HasFlag(ProviderType.FE),
                ShowTLevelsLinks = providerInfo.ProviderType.HasFlag(ProviderType.TLevels)
            };
        }
    }
}
