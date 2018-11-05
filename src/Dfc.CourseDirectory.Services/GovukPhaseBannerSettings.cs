using Dfc.CourseDirectory.Services.Interfaces;

namespace Dfc.CourseDirectory.Services
{
    public class GovukPhaseBannerSettings : IGovukPhaseBannerSettings
    {
        public bool IsVisible { get; set; }
        public string Tag { get; set; }
        public string LinkUrl { get; set; }
        public string LinkText { get; set; }
    }
}