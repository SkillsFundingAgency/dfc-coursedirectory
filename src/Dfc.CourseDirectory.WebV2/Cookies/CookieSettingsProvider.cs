using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.WebV2.Cookies
{
    public class CookieSettingsProvider : ICookieSettingsProvider
    {
        private const string CookieName = "cookie-settings";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public CookieSettingsProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public CookieSettings GetPreferencesForCurrentUser()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var cookieValue = request.Cookies[CookieName];

            return cookieValue != null && TryDeserializeSettings(cookieValue, out var settings) ?
                settings :
                null;
        }

        public void SetPreferencesForCurrentUser(CookieSettings preferences)
        {
            var response = _httpContextAccessor.HttpContext.Response;

            response.OnStarting(state =>
            {
                var (rsp, settings) = ((HttpResponse, CookieSettings))state;

                var serializedSettings = SerializeCookieSettings(settings);
                rsp.Cookies.Append(CookieName, serializedSettings);

                return Task.CompletedTask;
            }, (response, preferences));
        }

        private static bool TryDeserializeSettings(string value, out CookieSettings settings)
        {
            try
            {
                settings = JsonConvert.DeserializeObject<CookieSettings>(value);
                return true;
            }
            catch (JsonException)
            {
                settings = default;
                return false;
            }
        }

        private static string SerializeCookieSettings(CookieSettings settings) => JsonConvert.SerializeObject(settings);
    }
}
