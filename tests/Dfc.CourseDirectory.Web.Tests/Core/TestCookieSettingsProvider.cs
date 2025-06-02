using Dfc.CourseDirectory.WebV2.Cookies;

namespace Dfc.CourseDirectory.Web.Tests.Core
{
    public class TestCookieSettingsProvider : ICookieSettingsProvider
    {
        private CookieSettings _settings;

        public CookieSettings GetPreferencesForCurrentUser() => _settings;

        public void Reset() => _settings = null;

        public void SetPreferencesForCurrentUser(CookieSettings preferences) =>
            _settings = preferences;
    }
}
