namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface IGovukPhaseBannerService
    {
        IGovukPhaseBannerSettings GetSettings();

        IGovukPhaseBannerSettings GetSettings(bool? isVisible, string tag, string linkUrl, string linkText);
    }
}