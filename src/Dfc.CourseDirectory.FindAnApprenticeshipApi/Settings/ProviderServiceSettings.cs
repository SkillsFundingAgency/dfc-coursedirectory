using Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Settings;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Settings
{
    public class ProviderServiceSettings : IProviderServiceSettings
    {
        public ProviderServiceSettings()
        {
            CachePrefix = "Provider_";
            MinutesToCache = 23 * 60;
        }

        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
        public int MinutesToCache { get; set; }
        public string CachePrefix { get; set; }
    }
}
