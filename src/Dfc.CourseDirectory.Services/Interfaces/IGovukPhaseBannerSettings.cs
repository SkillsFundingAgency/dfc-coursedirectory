namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface IGovukPhaseBannerSettings
    {
        bool IsVisible { get; }
        string Tag { get; }
        string LinkUrl { get; }
        string LinkText { get; }
    }
}