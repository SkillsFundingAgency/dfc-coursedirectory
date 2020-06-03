namespace Dfc.CourseDirectory.WebV2.Cookies
{
    public interface ICookieSettingsProvider
    {
        CookieSettings GetPreferencesForCurrentUser();
        void SetPreferencesForCurrentUser(CookieSettings preferences);
    }
}
